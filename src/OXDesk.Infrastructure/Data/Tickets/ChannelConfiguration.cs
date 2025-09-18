using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
    {
        public void Configure(EntityTypeBuilder<Channel> builder)
        {
            builder.ToTable("channels");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);
            
            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(c => c.TenantId)
                .HasDatabaseName("ix_channels_tenant_id");
                
            // Composite index with tenant_id and ChannelKey
            builder.HasIndex(c => new { c.TenantId, c.ChannelKey })
                .HasDatabaseName("ix_channels_tenant_id_channel_key");
                
            // Keep original index for backward compatibility
            builder.HasIndex(c => c.ChannelKey);
        }
    }
}
