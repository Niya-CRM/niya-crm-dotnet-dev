using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.DynamicObjects.Fields;

namespace OXDesk.Infrastructure.Data.DynamicObjects.Fields;

public class DynamicObjectFieldConfiguration : IEntityTypeConfiguration<DynamicObjectField>
{
    public void Configure(EntityTypeBuilder<DynamicObjectField> builder)
    {
        builder.ToTable("dynamic_object_fields");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .UseIdentityByDefaultColumn()
               .HasIdentityOptions(startValue: 10001L);

        // Index for tenant_id for efficient multi-tenant filtering
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_dynamic_object_fields_tenant_id");
            
        // Index on object_key with tenant_id
        builder.HasIndex(x => new { x.TenantId, x.ObjectKey })
            .HasDatabaseName("ix_dynamic_object_fields_tenant_id_object_key");
            
        // Keep original index for backward compatibility
        builder.HasIndex(x => x.ObjectKey)
            .HasDatabaseName("ix_dynamic_object_fields_object_key");
    }
}
