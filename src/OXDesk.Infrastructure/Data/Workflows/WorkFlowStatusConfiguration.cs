using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Workflows;

namespace OXDesk.Infrastructure.Data.Workflows
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
                   .HasIdentityOptions(startValue: 101L);

            builder.HasIndex(x => x.WorkFlowId)
                   .HasDatabaseName("ix_workflow_statuses_workflow_id");
        }
    }
}
