using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Workflows;

namespace OXDesk.Infrastructure.Data.Workflows
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
                   .HasIdentityOptions(startValue: 101L);

            builder.HasIndex(x => x.ObjectId)
                   .HasDatabaseName("ix_workflows_object_id");
        }
    }
}
