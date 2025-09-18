using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.DynamicObjects;

namespace OXDesk.Infrastructure.Data.DynamicObjects
{
    /// <summary>
    /// Configuration for the DynamicObject entity.
    /// </summary>
    public class DynamicObjectConfiguration : IEntityTypeConfiguration<DynamicObject>
    {
        /// <summary>
        /// Configures the entity.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<DynamicObject> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);
            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(o => o.TenantId);
            
            // Add index on Key property with tenant_id
            builder.HasIndex(o => new { o.TenantId, o.ObjectKey })
                .IsUnique();
        }
    }
}
