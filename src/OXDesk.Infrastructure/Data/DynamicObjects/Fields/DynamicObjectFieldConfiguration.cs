using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.DynamicObjects.Fields;

namespace OXDesk.Infrastructure.Data.DynamicObjects.Fields;

/// <summary>
/// Entity Framework configuration for the DynamicObjectField entity.
/// Configures database table and column mappings.
/// </summary>
public class DynamicObjectFieldConfiguration : IEntityTypeConfiguration<DynamicObjectField>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<DynamicObjectField> builder)
    {
        builder.ToTable("dynamic_object_fields");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .UseIdentityByDefaultColumn()
               .HasIdentityOptions(startValue: 10001L);

        builder.HasIndex(x => x.ObjectId)
               .HasDatabaseName("ix_dynamic_object_fields_object_id");
    }
}
