using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// Entity Framework configuration for the Brand entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("brands");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);
            
            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(b => b.TenantId)
                .HasDatabaseName("ix_brands_tenant_id");
                
            // Composite index with tenant_id and BrandId
            builder.HasIndex(b => new { b.TenantId, b.Id })
                .HasDatabaseName("ix_brands_tenant_id_brand_id");
                
            // Keep original index for backward compatibility
            builder.HasIndex(b => b.Id);
        }
    }
}
