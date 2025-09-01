using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationUserLogin entity
/// </summary>
public class ApplicationUserLoginConfiguration : IEntityTypeConfiguration<ApplicationUserLogin>
{
    public void Configure(EntityTypeBuilder<ApplicationUserLogin> builder)
    {
        // Table name
        builder.ToTable("user_logins");

        // Primary key is already configured by Identity

        // Add tenant_id index
        builder.HasIndex(ul => ul.TenantId)
            .HasDatabaseName("ix_user_logins_tenant_id");
            
        // Add composite index with tenant_id and UserId
        builder.HasIndex(ul => new { ul.TenantId, ul.UserId })
            .HasDatabaseName("ix_user_logins_tenant_id_user_id");
            
        // Add composite unique index with tenant_id, LoginProvider, and ProviderKey
        builder.HasIndex(ul => new { ul.TenantId, ul.LoginProvider, ul.ProviderKey })
            .HasDatabaseName("ix_user_logins_tenant_id_login_provider_provider_key")
            .IsUnique();
    }
}
