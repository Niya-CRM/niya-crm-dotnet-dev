using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.FormTypes;

namespace OXDesk.Infrastructure.Data.FormTypes
{
    /// <summary>
    /// Entity Framework configuration for the Type entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class TypeConfiguration : IEntityTypeConfiguration<FormType>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<FormType> builder)
        {
            builder.ToTable("form_types");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 101L);

            builder.HasIndex(x => x.ObjectId)
                   .HasDatabaseName("ix_form_types_object_id");
        }
    }
}
