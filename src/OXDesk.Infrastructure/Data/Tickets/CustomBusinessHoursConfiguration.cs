using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets;

/// <summary>
/// Entity Framework configuration for the <see cref="CustomBusinessHours"/> entity.
/// </summary>
public class CustomBusinessHoursConfiguration : IEntityTypeConfiguration<CustomBusinessHours>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CustomBusinessHours> builder)
    {
        builder.ToTable("custom_business_hours");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 101L);

        builder.HasIndex(c => c.BusinessHourId)
            .HasDatabaseName("ix_custom_business_hours_business_hour_id");
    }
}
