using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Bsm.Api.Features.Leads;
using Bsm.Api.Features.LeadStatuses;

namespace Bsm.Api.Identity;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadStatus> LeadStatuses => Set<LeadStatus>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new LeadConfiguration());
        builder.ApplyConfiguration(new LeadStatusConfiguration());
    }
}