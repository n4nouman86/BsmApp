using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bsm.Api.Features.Leads;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads", "dbo");
        builder.HasKey(lead => lead.Id);
        builder.Property(lead => lead.Id).UseIdentityColumn();
        builder.Property(lead => lead.AppUserId).HasMaxLength(450);
        builder.Property(lead => lead.DateOfBirth).HasColumnType("date");
        builder.Property(lead => lead.SpouseDateOfBirth).HasColumnType("date");
        builder.Property(lead => lead.CreatedOn).HasColumnType("datetime2(3)");
        builder.HasOne(lead => lead.Status).WithMany().HasForeignKey(lead => lead.StatusId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(lead => lead.PhoneNumber).IsRequired();
        builder.Property(lead => lead.FirstName).IsRequired();
        builder.Property(lead => lead.LastName).IsRequired();
        builder.Property(lead => lead.Email).IsRequired();
        builder.Property(lead => lead.StreetAddress).IsRequired();
        builder.Property(lead => lead.State).IsRequired();
        builder.Property(lead => lead.HaveSpouse).IsRequired();
        builder.Property(lead => lead.CurrentAutoCarrier).IsRequired();
        builder.Property(lead => lead.AnyLaspse).IsRequired();
        builder.Property(lead => lead.MakeAndModel).IsRequired();
        builder.Property(lead => lead.Accidents).IsRequired();
        builder.Property(lead => lead.Tikets).IsRequired();
        builder.Property(lead => lead.HomeOwner).IsRequired();

        foreach (var property in builder.Metadata.GetProperties()
            .Where(property => property.ClrType == typeof(string) && property.Name != nameof(Lead.AppUserId)))
        {
            property.SetIsUnicode(false);
            property.SetMaxLength(property.Name switch
            {
                nameof(Lead.StreetAddress) => 256,
                nameof(Lead.MakeAndModel) => 1024,
                _ => 50,
            });
        }
    }
}
