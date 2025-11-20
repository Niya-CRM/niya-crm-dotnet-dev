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
                   .UseIdentityAlwaysColumn()
                   .HasIdentityOptions(startValue: 10001L);

            builder.HasIndex(o => o.ObjectKey)
                   .HasDatabaseName("ix_dynamic_objects_object_key")
                   .IsUnique();
        }
    }
}
