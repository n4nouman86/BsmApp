using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bsm.Api.Features.DncNumbers;

public sealed class DncNumberConfiguration : IEntityTypeConfiguration<DncNumber>
{
    public void Configure(EntityTypeBuilder<DncNumber> builder)
    {
        builder.ToTable("DncNumbers", "dbo");
        builder.HasKey(dnc => dnc.Id);
        builder.Property(dnc => dnc.Id).UseIdentityColumn();
        builder.Property(dnc => dnc.Phone).IsRequired().HasMaxLength(50).IsUnicode(false);
        builder.Property(dnc => dnc.FirstName).HasMaxLength(50).IsUnicode(false);
        builder.Property(dnc => dnc.LastName).HasMaxLength(50).IsUnicode(false);
        builder.Property(dnc => dnc.State).HasMaxLength(50).IsUnicode(false);
        builder.Property(dnc => dnc.CreatedOn).HasColumnType("datetime2(3)");
    }
}
