using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Workflows;

namespace OXDesk.Infrastructure.Data.Workflows
{
    /// <summary>
    /// EF Core configuration for WorkflowMapping entity.
    /// </summary>
    public sealed class WorkflowMappingConfiguration : IEntityTypeConfiguration<WorkflowMapping>
    {
        public void Configure(EntityTypeBuilder<WorkflowMapping> builder)
        {
            builder.ToTable("workflow_mappings");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 101L);

            builder.HasIndex(x => x.WorkFlowId)
                   .HasDatabaseName("ix_workflow_mappings_workflow_id");
        }
    }
}
