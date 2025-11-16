using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Tickets;

namespace OXDesk.Infrastructure.Data.Tickets
{
    /// <summary>
    /// Entity Framework configuration for the Brand entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("brands");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 10001L);
        }
    }
}
