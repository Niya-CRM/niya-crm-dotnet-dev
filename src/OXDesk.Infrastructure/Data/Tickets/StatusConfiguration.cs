using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// Entity Framework configuration for the Status entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class StatusConfiguration : IEntityTypeConfiguration<Status>
    {
        /// <inheritdoc/>
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
        }
    }
}
