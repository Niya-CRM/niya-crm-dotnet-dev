using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationUserToken entity
/// </summary>
public class ApplicationUserTokenConfiguration : IEntityTypeConfiguration<ApplicationUserToken>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ApplicationUserToken> builder)
    {
        // Table name
        builder.ToTable("user_tokens");

        // Add tenant_id index
        builder.HasIndex(ut => ut.TenantId)
            .HasDatabaseName("ix_user_tokens_tenant_id");
            
        // Add composite index with tenant_id and UserId
        builder.HasIndex(ut => new { ut.TenantId, ut.UserId })
            .HasDatabaseName("ix_user_tokens_tenant_id_user_id");
            
        // Add composite unique index with tenant_id, UserId, LoginProvider, and Name
        builder.HasIndex(ut => new { ut.TenantId, ut.UserId, ut.LoginProvider, ut.Name })
            .HasDatabaseName("ix_user_tokens_tenant_id_user_id_login_provider_name")
            .IsUnique();
    }
}
