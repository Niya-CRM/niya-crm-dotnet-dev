using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;
using OXDesk.Core.Helpers.Naming;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationRoleClaim entity
/// </summary>
public class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    {
        // Table name
        builder.ToTable("role_claims");
    }
}
