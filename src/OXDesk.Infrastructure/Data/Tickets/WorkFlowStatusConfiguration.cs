using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// EF Core configuration for WorkFlowStatus entity.
    /// </summary>
    public sealed class WorkFlowStatusConfiguration : IEntityTypeConfiguration<WorkFlowStatus>
    {
        public void Configure(EntityTypeBuilder<WorkFlowStatus> builder)
        {
            builder.ToTable("workflow_statuses");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);

            // Basic tenant index per project pattern
            builder.HasIndex(x => x.TenantId)
                   .HasDatabaseName("ix_workflow_statuses_tenant_id");
        }
    }
}
