using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// EF Core configuration for TicketStatus.
    /// </summary>
    public class TicketStatusConfiguration : IEntityTypeConfiguration<TicketStatus>
    {
        public void Configure(EntityTypeBuilder<TicketStatus> builder)
        {
            // Table mapping
            builder.ToTable("ticket_statuses");

            // Primary key
            builder.HasKey(s => s.Id);

            // Indexes
            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(s => s.TenantId)
                .HasDatabaseName("ix_ticket_statuses_tenant_id");
                
            // Composite index with tenant_id and StatusKey
            builder.HasIndex(s => new { s.TenantId, s.StatusKey })
                .HasDatabaseName("ix_ticket_statuses_tenant_id_status_key");
                
            // Keep original index for backward compatibility
            builder.HasIndex(s => s.StatusKey);
        }
    }
}
