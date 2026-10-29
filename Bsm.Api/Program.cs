using Bsm.Api.Identity;
using Bsm.Api.Features.Auth;
using Bsm.Api.Features.Tcpa;
using Bsm.Api.Features.Leads;
using Bsm.Api.Features.DncNames;
using Bsm.Api.Features.DncNumbers;
using Bsm.Api.Features.LeadStatuses;
using Bsm.Api.Features.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserBlockingService, UserBlockingService>();
builder.Services.Configure<TcpaOptions>(builder.Configuration.GetSection("Tcpa"));
builder.Services.AddHttpClient<ITcpaService, TcpaService>(client => client.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<ILeadStatusService, LeadStatusService>();
builder.Services.AddScoped<IDncNumberService, DncNumberService>();
builder.Services.AddScoped<IDncNameService, DncNameService>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 0;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("JWT signing key was not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero,
    };
    options.Events = new JwtBearerEvents
    {
        // Sliding expiration: once half of the token's lifetime has passed, hand the client a fresh token.
        OnTokenValidated = async context =>
        {
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var versionClaim = context.Principal?.FindFirst(TokenService.TokenVersionClaim)?.Value;
            if (userId is null || !int.TryParse(versionClaim, out var tokenVersion))
            {
                context.Fail("Invalid session.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == userId, context.HttpContext.RequestAborted);
            if (user is null || user.IsBlocked || user.TokenVersion != tokenVersion)
            {
                context.Fail("Session revoked.");
                return;
            }

            var token = context.SecurityToken;
            if (token.ValidFrom == DateTime.MinValue) return;
            var lifetime = token.ValidTo - token.ValidFrom;
            if (DateTime.UtcNow - token.ValidFrom < lifetime / 2) return;

            var refreshed = await context.HttpContext.RequestServices.GetRequiredService<ITokenService>().CreateAsync(user);
            context.Response.Headers["X-Refreshed-Token"] = refreshed.AccessToken;
        },
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebUi", policy => policy
        .WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "http://localhost:5174",
            "http://127.0.0.1:5173",
            "http://127.0.0.1:5174")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("X-Refreshed-Token"));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseExceptionHandler();

await SeedDevelopmentAdminAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("WebUi");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedDevelopmentAdminAsync(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        return;
    }

    using var scope = app.Services.CreateScope();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var adminSettings = configuration.GetSection("DevelopmentAdmin");
    var email = adminSettings["Email"]!;
    const string adminRole = "Admin";

    foreach (var role in new[] { adminRole, "Verifier" })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));
            }
        }
    }

    var user = await userManager.FindByEmailAsync(email);
    if (user is null)
    {
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = adminSettings["FirstName"] ?? "System",
            LastName = adminSettings["LastName"] ?? "Administrator",
        };

        var userResult = await userManager.CreateAsync(user, adminSettings["Password"]!);
        if (!userResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", userResult.Errors.Select(error => error.Description)));
        }
    }

    if (!await userManager.IsInRoleAsync(user, adminRole))
    {
        var roleResult = await userManager.AddToRoleAsync(user, adminRole);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));
        }
    }

    const string demoEmail = "demo@bsmjoin.test";
    var demoUser = await userManager.FindByEmailAsync(demoEmail);
    if (demoUser is null)
    {
        demoUser = new ApplicationUser
        {
            UserName = demoEmail,
            Email = demoEmail,
            EmailConfirmed = true,
            FirstName = "Demo",
            LastName = "Administrator",
        };

        var demoResult = await userManager.CreateAsync(demoUser, "Demo@12345");
        if (!demoResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", demoResult.Errors.Select(error => error.Description)));
        }
    }
    if (!await userManager.IsInRoleAsync(demoUser, adminRole))
    {
        var demoRoleResult = await userManager.AddToRoleAsync(demoUser, adminRole);
        if (!demoRoleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", demoRoleResult.Errors.Select(error => error.Description)));
        }
    }
}
