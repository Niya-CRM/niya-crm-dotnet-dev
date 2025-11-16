using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;
using OXDesk.Core.Helpers.Naming;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationRole entity
/// </summary>
public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        // Table name
        builder.ToTable("asp_net_roles");

        // Configure audit fields
        builder.Property(r => r.CreatedBy).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedBy).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();
    }
}
