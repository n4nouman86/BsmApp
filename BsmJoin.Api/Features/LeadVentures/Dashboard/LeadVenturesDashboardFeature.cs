using Microsoft.EntityFrameworkCore;

namespace BsmJoin.Api.Features.LeadVentures.Dashboard;

public static class LeadVenturesDashboardFeature
{
    public static IServiceCollection AddLeadVenturesDashboard(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("LeadVenturesConnection");
        if (string.IsNullOrWhiteSpace(connectionString) && environment.IsDevelopment())
        {
            var settingsPath = Path.GetFullPath(Path.Combine(
                environment.ContentRootPath, "..", "Apps", "LeadVentures", "BlazorPortal"));
            var localConfiguration = new ConfigurationBuilder()
                .SetBasePath(settingsPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
            connectionString = localConfiguration.GetConnectionString("DefaultConnection");
        }

        services.AddDbContext<LeadVenturesReportDbContext>(options =>
            options.UseSqlServer(connectionString
                ?? throw new InvalidOperationException("Connection string 'LeadVenturesConnection' was not configured.")));
        services.AddScoped<ILeadVenturesReportService, LeadVenturesReportService>();
        return services;
    }
}
