using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Bsm.Api.Features.DncNames;
using Bsm.Api.Features.DncNumbers;
using Bsm.Api.Features.Leads;
using Bsm.Api.Features.LeadStatuses;

namespace Bsm.Api.Identity;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadStatus> LeadStatuses => Set<LeadStatus>();
    public DbSet<DncNumber> DncNumbers => Set<DncNumber>();
    public DbSet<DncName> DncNames => Set<DncName>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new LeadConfiguration());
        builder.ApplyConfiguration(new LeadStatusConfiguration());
        builder.ApplyConfiguration(new DncNumberConfiguration());
        builder.ApplyConfiguration(new DncNameConfiguration());
    }
}