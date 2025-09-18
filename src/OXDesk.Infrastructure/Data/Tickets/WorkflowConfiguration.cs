using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// EF Core configuration for Workflow entity.
    /// </summary>
    public sealed class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
    {
        public void Configure(EntityTypeBuilder<Workflow> builder)
        {
            builder.ToTable("workflows");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);

            // Indexes for multi-tenancy and query performance
            builder.HasIndex(x => x.TenantId)
                   .HasDatabaseName("ix_workflows_tenant_id");
        }
    }
}
