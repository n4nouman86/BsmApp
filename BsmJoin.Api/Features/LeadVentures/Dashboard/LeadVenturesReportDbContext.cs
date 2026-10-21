using Microsoft.EntityFrameworkCore;

namespace BsmJoin.Api.Features.LeadVentures.Dashboard;

public sealed class LeadVenturesReportDbContext(DbContextOptions<LeadVenturesReportDbContext> options)
    : DbContext(options);
