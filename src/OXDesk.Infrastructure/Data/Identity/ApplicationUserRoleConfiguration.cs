using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;
using OXDesk.Core.Helpers.Naming;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationUserRole entity
/// </summary>
public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        // Table name
        builder.ToTable("user_roles");
    }
}
