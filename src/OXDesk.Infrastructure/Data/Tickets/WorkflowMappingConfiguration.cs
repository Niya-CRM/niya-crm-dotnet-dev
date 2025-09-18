using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
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
                   .HasIdentityOptions(startValue: 10001L);

            // Multi-tenant indexes
            builder.HasIndex(x => x.TenantId)
                   .HasDatabaseName("ix_workflow_mappings_tenant_id");

            builder.HasIndex(x => new { x.TenantId, x.TopicId, x.SubTopicId })
                   .IsUnique()
                   .HasDatabaseName("ux_workflow_mappings_tenant_id_topic_id_sub_topic_id");
        }
    }
}
