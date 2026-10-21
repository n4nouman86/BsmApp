using Microsoft.EntityFrameworkCore;

namespace BsmJoin.Api.Features.LeadLogix.Dashboard;

public static class LeadLogixDashboardFeature
{
    public static IServiceCollection AddLeadLogixDashboard(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("LeadLogixConnection");
        if (string.IsNullOrWhiteSpace(connectionString) && environment.IsDevelopment())
        {
            var settingsPath = Path.GetFullPath(Path.Combine(
                environment.ContentRootPath, "..", "Apps", "LeadLogix", "BlazorPortal"));
            var localConfiguration = new ConfigurationBuilder()
                .SetBasePath(settingsPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
            connectionString = localConfiguration.GetConnectionString("DefaultConnection");
        }

        services.AddDbContext<LeadLogixReportDbContext>(options =>
            options.UseSqlServer(connectionString
                ?? throw new InvalidOperationException("Connection string 'LeadLogixConnection' was not configured.")));
        services.AddScoped<ILeadLogixReportService, LeadLogixReportService>();
        return services;
    }
}
