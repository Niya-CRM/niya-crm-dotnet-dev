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
            // Configure int identity primary key starting at 10001
            builder.Property(v => v.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);

            // Map relationship by ListId (FK) to ValueList.Id (principal key)
            builder
                .HasOne(vli => vli.ValueList)
                .WithMany()
                .HasForeignKey(vli => vli.ListId)
                .HasPrincipalKey(vl => vl.Id);
        }
    }
}
