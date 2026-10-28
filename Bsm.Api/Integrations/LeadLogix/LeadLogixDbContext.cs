using Microsoft.EntityFrameworkCore;

namespace Bsm.Api.Integrations.LeadLogix;

public sealed class LeadLogixDbContext(DbContextOptions<LeadLogixDbContext> options)
    : DbContext(options);
