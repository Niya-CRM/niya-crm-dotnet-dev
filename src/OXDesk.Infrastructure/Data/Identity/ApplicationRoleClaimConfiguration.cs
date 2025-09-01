using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;
using OXDesk.Core.Helpers.Naming;

namespace OXDesk.Infrastructure.Data.Identity;

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
        
        // Add tenant_id index
        builder.HasIndex(rc => rc.TenantId)
            .HasDatabaseName("ix_role_claims_tenant_id");
            
        // Add composite index with tenant_id and RoleId
        builder.HasIndex(rc => new { rc.TenantId, rc.RoleId })
            .HasDatabaseName("ix_role_claims_tenant_id_role_id");
    }
}
