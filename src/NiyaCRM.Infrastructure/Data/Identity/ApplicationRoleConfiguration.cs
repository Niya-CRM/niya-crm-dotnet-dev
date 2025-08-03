using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.Helpers.Naming;

namespace NiyaCRM.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationRole entity
/// </summary>
public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        // Table name
        builder.ToTable("asp_net_roles");

        // Primary key is already configured by Identity

        // Configure audit fields
        builder.Property(r => r.CreatedBy).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedBy).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();
    }
}
