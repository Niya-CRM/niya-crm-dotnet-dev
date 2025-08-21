using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    public class RequestTypeConfiguration : IEntityTypeConfiguration<RequestType>
    {
        public void Configure(EntityTypeBuilder<RequestType> builder)
        {
            builder.ToTable("request_types");
            builder.HasKey(r => r.Id);
            builder.HasIndex(r => r.RequestTypeKey);
        }
    }
}
