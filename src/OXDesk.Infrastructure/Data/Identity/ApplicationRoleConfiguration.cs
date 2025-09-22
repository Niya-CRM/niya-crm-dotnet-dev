using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;
using OXDesk.Core.Helpers.Naming;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationRole entity
/// </summary>
public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        // Table name
        builder.ToTable("asp_net_roles");

        // Start identity values for role Ids at 10001
        builder.Property(r => r.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 10001L);

        // Primary key is already configured by Identity

        // Configure audit fields
        builder.Property(r => r.CreatedBy).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedBy).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();
        
        // Add tenant_id index
        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_asp_net_roles_tenant_id");
            
        // Add composite index with tenant_id and NormalizedName
        builder.HasIndex(r => new { r.TenantId, r.NormalizedName })
            .HasDatabaseName("ix_asp_net_roles_tenant_id_normalized_name")
            .IsUnique();
    }
}
