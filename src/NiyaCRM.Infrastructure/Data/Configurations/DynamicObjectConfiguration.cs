using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.DynamicObjects;

namespace NiyaCRM.Infrastructure.Data.Configurations
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
            // Add index on Key property
            builder.HasIndex(o => o.Key)
                .IsUnique();
        }
    }
}
