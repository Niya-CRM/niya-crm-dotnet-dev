using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationUserClaim entity
/// </summary>
public class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
    {
        // Table name
        builder.ToTable("user_claims");

        // Primary key is already configured by Identity
    }
}
