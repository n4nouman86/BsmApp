using BlazorPortal.Components;
using BlazorPortal.Components.Account;
using BlazorPortal.Data;
using BlazorPortal.Hubs;
using BlazorPortal.PaymentMethods;
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

var logixString = builder.Configuration.GetConnectionString("LogixConnection")
	?? throw new InvalidOperationException("Connection string 'LogixConnection' not found.");

builder.Services.AddPooledDbContextFactory<LogixDbContext>(options => options.UseSqlServer(logixString));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
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
builder.Services.AddScoped<DialogService>();
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailSender<ApplicationUser>, EmailService>();
builder.Services.AddScoped<PaymentRequestEmailService>();
builder.Services.AddScoped<TcpaService>();

builder.Services.AddHttpClient<MastercardService>();

builder.Services.AddScoped<AppUserService>();
builder.Services.AddScoped<OscarService>();
builder.Services.AddScoped<MiretService>();
builder.Services.AddScoped<EstersonService>();
builder.Services.AddScoped<MikeService>();
builder.Services.AddScoped<WyattService>();
builder.Services.AddScoped<MarkMercerService>();
builder.Services.AddScoped<JayDepoloService>();
builder.Services.AddScoped<AdamWingService>();
builder.Services.AddScoped<LisaFainaService>();
builder.Services.AddScoped<LisaFainaService2>();
builder.Services.AddScoped<JorgeCarbonellService>();
builder.Services.AddScoped<StefanSmallwoodService>();


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
