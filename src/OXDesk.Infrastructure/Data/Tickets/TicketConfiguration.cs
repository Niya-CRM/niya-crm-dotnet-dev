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
