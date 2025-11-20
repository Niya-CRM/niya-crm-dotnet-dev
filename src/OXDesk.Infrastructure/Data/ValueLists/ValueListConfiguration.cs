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
            // Configure int identity primary key starting at 101
            builder.Property(v => v.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 101L);

            builder.HasIndex(x => x.ListName)
                   .HasDatabaseName("ix_value_lists_list_name");
        }
    }
}

