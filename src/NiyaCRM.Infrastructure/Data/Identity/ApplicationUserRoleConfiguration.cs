using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.Helpers.Naming;

namespace NiyaCRM.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationUserRole entity
/// </summary>
public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        // Table name
        builder.ToTable("user_roles");

        // Primary key is already configured by Identity

        // Configure audit fields
        builder.Property(ur => ur.CreatedBy).IsRequired();
        builder.Property(ur => ur.CreatedAt).IsRequired();
        builder.Property(ur => ur.UpdatedBy).IsRequired();
        builder.Property(ur => ur.UpdatedAt).IsRequired();
    }
}
