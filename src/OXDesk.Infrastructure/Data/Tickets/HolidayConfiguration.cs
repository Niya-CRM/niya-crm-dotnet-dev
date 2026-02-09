using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets;

/// <summary>
/// Entity Framework configuration for the <see cref="Holiday"/> entity.
/// </summary>
public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("holidays");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 101L);

        builder.HasIndex(h => h.BusinessHourId)
            .HasDatabaseName("ix_holidays_business_hour_id");
    }
}
