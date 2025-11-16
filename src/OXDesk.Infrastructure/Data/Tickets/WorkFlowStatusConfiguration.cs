using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// Entity Framework configuration for the WorkFlowStatus entity.
    /// Configures database table and column mappings.
    /// </summary>
    public sealed class WorkFlowStatusConfiguration : IEntityTypeConfiguration<WorkFlowStatus>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<WorkFlowStatus> builder)
        {
            builder.ToTable("workflow_statuses");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);
        }
    }
}
