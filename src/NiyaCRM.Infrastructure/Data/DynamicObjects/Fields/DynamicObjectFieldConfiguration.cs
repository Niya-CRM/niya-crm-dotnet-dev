using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.DynamicObjects.Fields;

namespace NiyaCRM.Infrastructure.Data.DynamicObjects.Fields;

public class DynamicObjectFieldConfiguration : IEntityTypeConfiguration<DynamicObjectField>
{
    public void Configure(EntityTypeBuilder<DynamicObjectField> builder)
    {
        builder.ToTable("dynamic_object_fields");
        builder.HasKey(x => x.Id);

        // Index on object_key
        builder.HasIndex(x => x.ObjectKey)
            .HasDatabaseName("ix_dynamic_object_fields_object_key");
    }
}
