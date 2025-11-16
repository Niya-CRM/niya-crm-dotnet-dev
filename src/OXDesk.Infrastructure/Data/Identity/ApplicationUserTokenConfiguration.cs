using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for ApplicationUserToken entity
/// </summary>
public class ApplicationUserTokenConfiguration : IEntityTypeConfiguration<ApplicationUserToken>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ApplicationUserToken> builder)
    {
        // Table name
        builder.ToTable("user_tokens");
    }
}
