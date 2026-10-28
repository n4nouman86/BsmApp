using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bsm.Api.Features.LeadStatuses;

public sealed class LeadStatusConfiguration : IEntityTypeConfiguration<LeadStatus>
{
    public void Configure(EntityTypeBuilder<LeadStatus> builder)
    {
        builder.ToTable("LeadStatuses", "dbo");
        builder.HasKey(status => status.Id);
        builder.Property(status => status.Id).UseIdentityColumn();
        builder.Property(status => status.Name).IsRequired().HasMaxLength(50).IsUnicode(false);
        builder.Property(status => status.CreatedOn).HasColumnType("datetime2(3)");
    }
}
