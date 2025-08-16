using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.ValueLists;

namespace NiyaCRM.Infrastructure.Data.ValueLists
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
            // Configure ListKey as an alternate key (unique constraint)
            builder.HasAlternateKey(v => v.ListKey);

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

