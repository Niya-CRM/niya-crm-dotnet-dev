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
            builder.HasIndex(s => s.StatusKey);
        }
    }
}
