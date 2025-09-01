using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("brands");
            builder.HasKey(b => b.Id);
            
            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(b => b.TenantId)
                .HasDatabaseName("ix_brands_tenant_id");
                
            // Composite index with tenant_id and BrandKey
            builder.HasIndex(b => new { b.TenantId, b.BrandKey })
                .HasDatabaseName("ix_brands_tenant_id_brand_key");
                
            // Keep original index for backward compatibility
            builder.HasIndex(b => b.BrandKey);
        }
    }
}
