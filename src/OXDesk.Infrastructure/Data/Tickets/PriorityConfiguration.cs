using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    public class PriorityConfiguration : IEntityTypeConfiguration<Priority>
    {
        public void Configure(EntityTypeBuilder<Priority> builder)
        {
            builder.ToTable("priorities");
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.PriorityKey);
        }
    }
}
