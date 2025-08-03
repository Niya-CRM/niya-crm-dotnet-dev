using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.Helpers.Naming;

namespace NiyaCRM.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationRoleClaim entity
/// </summary>
public class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    {
        // Table name
        builder.ToTable("role_claims");

        // Primary key is already configured by Identity

        // Configure audit fields
        builder.Property(rc => rc.CreatedBy).IsRequired();
        builder.Property(rc => rc.CreatedAt).IsRequired();
        builder.Property(rc => rc.UpdatedBy).IsRequired();
        builder.Property(rc => rc.UpdatedAt).IsRequired();
    }
}
