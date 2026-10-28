using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bsm.Api.Features.DncNames;

public sealed class DncNameConfiguration : IEntityTypeConfiguration<DncName>
{
    public void Configure(EntityTypeBuilder<DncName> builder)
    {
        builder.ToTable("DncNames", "dbo");
        builder.HasKey(dnc => dnc.Id);
        builder.Property(dnc => dnc.Id).UseIdentityColumn();
        builder.Property(dnc => dnc.FirstName).IsRequired().HasMaxLength(50).IsUnicode(false);
        builder.Property(dnc => dnc.LastName).IsRequired().HasMaxLength(50).IsUnicode(false);
        builder.Property(dnc => dnc.Phone).HasMaxLength(50).IsUnicode(false);
        builder.Property(dnc => dnc.State).HasMaxLength(50).IsUnicode(false);
    }
}
