using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Settings;

namespace OXDesk.Infrastructure.Data.Settings;

/// <summary>
/// Entity Framework configuration for the <see cref="Setting"/> entity.
/// </summary>
public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("settings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 101L);

        builder.HasIndex(s => s.Key)
            .IsUnique()
            .HasDatabaseName("ix_settings_key");
    }
}
