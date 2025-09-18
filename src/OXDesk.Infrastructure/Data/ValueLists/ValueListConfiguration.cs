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
            // Configure int identity primary key starting at 10001
            builder.Property(v => v.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);

            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(v => v.TenantId)
                .HasDatabaseName("ix_value_lists_tenant_id");

            // Optional: explicit property configuration (attributes already set types)
            builder.Property(v => v.ListName)
                   .IsRequired()
                   .HasMaxLength(50);
        }
    }
}

