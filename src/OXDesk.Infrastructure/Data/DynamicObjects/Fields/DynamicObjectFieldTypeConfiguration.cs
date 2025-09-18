using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.DynamicObjects.Fields;

namespace OXDesk.Infrastructure.Data.DynamicObjects.Fields
{
    /// <summary>
    /// EF Core configuration for DynamicObjectFieldType.
    /// Note: Field types are global (no tenant_id) per project rules.
    /// </summary>
    public class DynamicObjectFieldTypeConfiguration : IEntityTypeConfiguration<DynamicObjectFieldType>
    {
        public void Configure(EntityTypeBuilder<DynamicObjectFieldType> builder)
        {
            builder.ToTable("dynamic_object_field_types");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);

            builder.HasIndex(x => x.FieldTypeKey)
                   .HasDatabaseName("ix_dynamic_object_field_types_field_type_key");
        }
    }
}
