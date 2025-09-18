using System;
using System.ComponentModel.DataAnnotations;

namespace OXDesk.Core.Identity
{
    /// <summary>
    /// User refresh token entity used to issue new access tokens.
    /// Stores only a hashed representation of the refresh token for security.
    /// </summary>
    public class UserRefreshToken
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the tenant identifier.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column("tenant_id")]
        public Guid? TenantId { get; set; }

        /// <summary>
        /// The user this refresh token belongs to.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The hashed value of the refresh token. Never store the plaintext token.
        /// </summary>
        [MaxLength(200)]
        public required string HashedToken { get; set; }

        /// <summary>
        /// Optional device information (e.g., browser/OS) the token was issued to.
        /// </summary>
        [MaxLength(255)]
        public string? Device { get; set; }

        /// <summary>
        /// IP address from which the token was created.
        /// </summary>
        [MaxLength(60)] // supports IPv6
        public string? IpAddress { get; set; }

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp (UTC).
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Expiration timestamp (UTC). After this time the refresh token is invalid.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// The timestamp (UTC) when this refresh token was first used. Null if never used.
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// Number of times this refresh token has been used to generate new tokens.
        /// </summary>
        public int UsedCounter { get; set; } = 0;

        /// <summary>
        /// Navigation to ApplicationUser.
        /// </summary>
        public ApplicationUser? User { get; set; }
    }
}
