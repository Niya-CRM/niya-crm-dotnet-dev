using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.ValueLists;

namespace NiyaCRM.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configuration for the ValueListItem entity.
    /// </summary>
    public class ValueListItemConfiguration : IEntityTypeConfiguration<ValueListItem>
    {
        /// <summary>
        /// Configures the entity.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<ValueListItem> builder)
        {
            // Add index on Key property
            builder.HasIndex(v => v.Key)
                .IsUnique();
        }
    }
}
