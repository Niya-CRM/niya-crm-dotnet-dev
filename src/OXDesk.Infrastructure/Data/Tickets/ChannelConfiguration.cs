using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// Entity Framework configuration for the Channel entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Channel> builder)
        {
            builder.ToTable("channels");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);
        }
    }
}
