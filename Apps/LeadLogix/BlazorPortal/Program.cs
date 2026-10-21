using BlazorPortal.Components;
using BlazorPortal.Components.Account;
using BlazorPortal.Data;
using BlazorPortal.Hubs;
using BlazorPortal.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Radzen;
using System.Globalization;
using WebPortal.Services;

var builder = WebApplication.CreateBuilder(args);

var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents(options =>
	{
		options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(30);
		options.DisconnectedCircuitMaxRetained = 1000;
	})
	.AddHubOptions(options =>
	{
		options.KeepAliveInterval = TimeSpan.FromSeconds(10);
		options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
	});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
});

var ventureConnection = builder.Configuration.GetConnectionString("VentureConnection")
	?? throw new InvalidOperationException("Connection string 'VentureConnection' not found.");

builder.Services.AddPooledDbContextFactory<VentureDbContext>(options => options.UseSqlServer(ventureConnection));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString))
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(option =>
{
    option.Password.RequiredUniqueChars = 0;
    option.Password.RequireDigit = false;
    option.Password.RequireLowercase = false;
    option.Password.RequireNonAlphanumeric = false;
    option.Password.RequireUppercase = false;
    option.Password.RequiredLength = 6;
});

builder.Services.AddRadzenComponents();
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailSender<ApplicationUser>, EmailService>();
builder.Services.AddScoped<AppUserService>();
builder.Services.AddScoped<TcpaService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.MapHub<LeadHub>("/leadhub");
app.MapHub<LeadCommentHub>("/leadcommenthub");

app.Run();
