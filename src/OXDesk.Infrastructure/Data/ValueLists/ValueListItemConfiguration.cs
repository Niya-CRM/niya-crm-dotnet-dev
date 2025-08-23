using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.ValueLists;

namespace OXDesk.Infrastructure.Data.ValueLists
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
            // Non-unique index on ItemKey
            builder.HasIndex(v => v.ItemKey);

            // Index on ListKey
            builder.HasIndex(v => v.ListKey);

            // Map relationship by ListKey (FK) to ValueList.ListKey (principal key)
            builder
                .HasOne(vli => vli.ValueList)
                .WithMany()
                .HasForeignKey(vli => vli.ListKey)
                .HasPrincipalKey(vl => vl.ListKey);
        }
    }
}
