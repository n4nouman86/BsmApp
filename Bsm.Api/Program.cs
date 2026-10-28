using Bsm.Api.Identity;
using Bsm.Api.Features.Auth;
using Bsm.Api.Features.Tcpa;
using Bsm.Api.Features.Leads;
using Bsm.Api.Features.DncNames;
using Bsm.Api.Features.DncNumbers;
using Bsm.Api.Features.LeadStatuses;
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

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
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
            var token = context.SecurityToken;
            if (token.ValidFrom == DateTime.MinValue) return;
            var lifetime = token.ValidTo - token.ValidFrom;
            if (DateTime.UtcNow - token.ValidFrom < lifetime / 2) return;

            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId is null) return;
            var user = await context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(userId);
            if (user is null) { context.Fail("User no longer exists."); return; }

            var refreshed = await context.HttpContext.RequestServices.GetRequiredService<ITokenService>().CreateAsync(user);
            context.Response.Headers["X-Refreshed-Token"] = refreshed.AccessToken;
        },
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebUi", policy => policy
        .WithOrigins("http://localhost:5173", "https://localhost:5173", "http://localhost:5174")
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

    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRole));
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));
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
