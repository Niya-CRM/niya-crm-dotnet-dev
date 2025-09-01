using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.ValueLists;

namespace OXDesk.Infrastructure.Data.ValueLists
{
    /// <summary>
    /// Configuration for the ValueList entity.
    /// </summary>
    public class ValueListConfiguration : IEntityTypeConfiguration<ValueList>
    {
        /// <summary>
        /// Configures the entity.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<ValueList> builder)
        {
            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(v => v.TenantId)
                .HasDatabaseName("ix_value_lists_tenant_id");
                
            // Composite index with tenant_id and ListKey
            builder.HasIndex(v => new { v.TenantId, v.ListKey })
                .HasDatabaseName("ix_value_lists_tenant_id_list_key")
                .IsUnique();

            // Optional: explicit property configuration (attributes already set types)
            builder.Property(v => v.ListName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(v => v.ListKey)
                   .IsRequired()
                   .HasMaxLength(60);
        }
    }
}

