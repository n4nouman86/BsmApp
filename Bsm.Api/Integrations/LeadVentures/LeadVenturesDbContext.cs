using Microsoft.EntityFrameworkCore;

namespace Bsm.Api.Integrations.LeadVentures;

public sealed class LeadVenturesDbContext(DbContextOptions<LeadVenturesDbContext> options)
    : DbContext(options);
