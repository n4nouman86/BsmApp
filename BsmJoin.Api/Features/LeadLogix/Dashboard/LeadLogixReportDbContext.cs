using Microsoft.EntityFrameworkCore;

namespace BsmJoin.Api.Features.LeadLogix.Dashboard;

public sealed class LeadLogixReportDbContext(DbContextOptions<LeadLogixReportDbContext> options)
    : DbContext(options);
