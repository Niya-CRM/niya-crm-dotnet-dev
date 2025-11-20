using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Workflows;

namespace OXDesk.Infrastructure.Data.Statuses
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
            builder.ToTable("statuses");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 101L);

            builder.HasIndex(x => x.ObjectId)
                   .HasDatabaseName("ix_statuses_object_id");
        }
    }
}
