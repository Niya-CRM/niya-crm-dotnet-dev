using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.Helpers.Naming;

namespace NiyaCRM.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for Permission entity
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        // Table name
        builder.ToTable("permissions");

        // Primary key
        builder.HasKey(p => p.Id);

        // Indexes
        builder.HasIndex(p => p.NormalizedName)
            .IsUnique();
    }
}
