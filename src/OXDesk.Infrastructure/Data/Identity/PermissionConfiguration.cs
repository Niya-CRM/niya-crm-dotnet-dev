using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;
using OXDesk.Core.Helpers.Naming;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for Permission entity
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        // Table name
        builder.ToTable("permissions");

        // Primary key
        builder.HasKey(p => p.Id);

        // Configure Id to start from 10001
        builder.Property(p => p.Id)
               .UseIdentityByDefaultColumn()
               .HasIdentityOptions(startValue: 10001L);
    }
}
