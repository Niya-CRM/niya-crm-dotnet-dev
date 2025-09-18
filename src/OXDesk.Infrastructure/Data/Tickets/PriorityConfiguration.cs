using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    public class PriorityConfiguration : IEntityTypeConfiguration<Priority>
    {
        public void Configure(EntityTypeBuilder<Priority> builder)
        {
            builder.ToTable("priorities");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);
            
            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(p => p.TenantId)
                .HasDatabaseName("ix_priorities_tenant_id");
                
            // Composite index with tenant_id and PriorityKey
            builder.HasIndex(p => new { p.TenantId, p.PriorityKey })
                .HasDatabaseName("ix_priorities_tenant_id_priority_key");
                
            // Keep original index for backward compatibility
            builder.HasIndex(p => p.PriorityKey);
        }
    }
}
