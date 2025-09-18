using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.Identity;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Entity configuration for UserRefreshToken entity.
/// </summary>
public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        // Table name mapping to preserve existing schema
        builder.ToTable("user_refresh_tokens");

        // Indexes for housekeeping and query performance
        builder
            .HasIndex(e => e.UsedAt)
            .HasDatabaseName("ix_user_refresh_tokens_used_at");

        builder
            .HasIndex(e => e.ExpiresAt)
            .HasDatabaseName("ix_user_refresh_tokens_expires_at");
    }
}
