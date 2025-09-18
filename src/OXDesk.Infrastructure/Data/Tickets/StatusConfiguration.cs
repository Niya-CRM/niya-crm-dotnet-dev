using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// EF Core configuration for TicketStatus.
    /// </summary>
    public class StatusConfiguration : IEntityTypeConfiguration<Status>
    {
        public void Configure(EntityTypeBuilder<Status> builder)
        {
            // Table mapping
            builder.ToTable("statuses");

            // Primary key
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);

            // Indexes
            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(s => s.TenantId)
                .HasDatabaseName("ix_statuses_tenant_id");
        }
    }
}
