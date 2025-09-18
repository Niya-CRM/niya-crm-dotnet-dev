using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// Entity Framework configuration for the Ticket entity.
    /// Configures table mapping, indexes, and provider-specific behaviors.
    /// </summary>
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            // Table name (snake_case handled globally as well)
            builder.ToTable("tickets");

            // Primary key
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10000001L);

            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(t => t.TenantId)
                .HasDatabaseName("ix_tickets_tenant_id");
            
            // Composite index with tenant_id and TicketNumber
            builder.HasIndex(t => new { t.TenantId, t.TicketNumber })
                .HasDatabaseName("ix_tickets_tenant_id_ticket_number")
                .IsUnique();
            
            // Composite indexes with tenant_id for multi-tenant filtering
            builder.HasIndex(t => new { t.TenantId, t.BrandKey })
                .HasDatabaseName("ix_tickets_tenant_id_brand_key");
            builder.HasIndex(t => new { t.TenantId, t.ChannelKey})
                .HasDatabaseName("ix_tickets_tenant_id_channel_key");
            builder.HasIndex(t => new { t.TenantId, t.Owner })
                .HasDatabaseName("ix_tickets_tenant_id_owner");
            builder.HasIndex(t => new { t.TenantId, t.Team })
                .HasDatabaseName("ix_tickets_tenant_id_team");
            builder.HasIndex(t => new { t.TenantId, t.Organisation })
                .HasDatabaseName("ix_tickets_tenant_id_organisation");
            builder.HasIndex(t => new { t.TenantId, t.StatusKey })
                .HasDatabaseName("ix_tickets_tenant_id_status_key");
            builder.HasIndex(t => new { t.TenantId, t.CreatedAt })
                .HasDatabaseName("ix_tickets_tenant_id_created_at");
            builder.HasIndex(t => new { t.TenantId, t.DeletedAt })
                .HasDatabaseName("ix_tickets_tenant_id_deleted_at");
            builder.HasIndex(t => new { t.TenantId, t.StatusKey, t.CreatedAt, t.DueAt, t.DeletedAt })
                .HasDatabaseName("ix_tickets_tenant_id_status_created_due_deleted");

            // Defaults for counters and flags (DB-level)
            builder.Property(t => t.PriorityScore).HasDefaultValue(1);
            builder.Property(t => t.AttachmentCount).HasDefaultValue(0);
            builder.Property(t => t.CommentCount).HasDefaultValue(0);
            builder.Property(t => t.TaskCount).HasDefaultValue(0);
            builder.Property(t => t.ThreadCount).HasDefaultValue(0);

            builder.Property(t => t.IsEscalated).HasDefaultValue(false);
            builder.Property(t => t.IsSpam).HasDefaultValue(false);
            builder.Property(t => t.IsArchived).HasDefaultValue(false);
            builder.Property(t => t.IsDeleted).HasDefaultValue(false);
            builder.Property(t => t.IsAutoClosed).HasDefaultValue(false);
            builder.Property(t => t.IsRead).HasDefaultValue(false);
            builder.Property(t => t.HasScheduledReply).HasDefaultValue(false);
            builder.Property(t => t.IsResponseOverdue).HasDefaultValue(false);
            builder.Property(t => t.IsOverdue).HasDefaultValue(false);
            builder.Property(t => t.IsReopened).HasDefaultValue(false);
        }
    }
}
