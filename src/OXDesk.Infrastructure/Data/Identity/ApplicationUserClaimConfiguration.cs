using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationUserClaim entity
/// </summary>
public class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
    {
        // Table name
        builder.ToTable("user_claims");

        // Primary key is already configured by Identity

        // Add tenant_id index
        builder.HasIndex(uc => uc.TenantId)
            .HasDatabaseName("ix_user_claims_tenant_id");
            
        // Add composite index with tenant_id and UserId
        builder.HasIndex(uc => new { uc.TenantId, uc.UserId })
            .HasDatabaseName("ix_user_claims_tenant_id_user_id");
    }
}
