using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;
using OXDesk.Core.Helpers.Naming;

namespace OXDesk.Infrastructure.Data.Identity;

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
        
        // Add tenant_id index
        builder.HasIndex(ur => ur.TenantId)
            .HasDatabaseName("ix_user_roles_tenant_id");
            
        // Add composite index with tenant_id, UserId, and RoleId
        builder.HasIndex(ur => new { ur.TenantId, ur.UserId, ur.RoleId })
            .HasDatabaseName("ix_user_roles_tenant_id_user_id_role_id")
            .IsUnique();
    }
}
