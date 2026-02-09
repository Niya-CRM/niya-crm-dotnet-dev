using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets;

/// <summary>
/// Entity Framework configuration for the <see cref="BusinessHours"/> entity.
/// </summary>
public class BusinessHoursConfiguration : IEntityTypeConfiguration<BusinessHours>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BusinessHours> builder)
    {
        builder.ToTable("business_hours");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 101L);

        builder.HasIndex(b => b.Name)
            .HasDatabaseName("ix_business_hours_name");
    }
}
