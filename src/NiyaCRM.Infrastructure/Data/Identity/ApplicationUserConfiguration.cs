using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.Identity;

namespace NiyaCRM.Infrastructure.Data.Identity
{
    /// <summary>
    /// EF Core configuration for ApplicationUser.
    /// </summary>
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Location: required, max length 60, default empty string to support existing rows during migration
            builder.Property(u => u.Location)
                   .HasMaxLength(60)
                   .HasColumnType("varchar(60)")
                   .IsRequired()
                   .HasDefaultValue(string.Empty);
        }
    }
}
