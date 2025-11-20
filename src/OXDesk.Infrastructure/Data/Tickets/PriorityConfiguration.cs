using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// Entity Framework configuration for the Priority entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class PriorityConfiguration : IEntityTypeConfiguration<Priority>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Priority> builder)
        {
            builder.ToTable("priorities");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 101L);

            builder.HasIndex(p => p.PriorityName)
                   .HasDatabaseName("ix_priorities_priority_name");
        }
    }
}
