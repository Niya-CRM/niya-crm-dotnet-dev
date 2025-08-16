using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.AppInstallation;

namespace OXDesk.Infrastructure.Data.AppInstallation;

/// <summary>
/// Entity configuration for AppInstallationStatus.
/// </summary>
public class AppInstallationStatusConfiguration : IEntityTypeConfiguration<AppInstallationStatus>
{
    /// <summary>
    /// Configures the entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<AppInstallationStatus> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Version);
    }
}
