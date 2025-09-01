using System;
using System.ComponentModel.DataAnnotations;

namespace OXDesk.Core.Identity
{
    /// <summary>
    /// Refresh token entity used to issue new access tokens.
    /// Stores only a hashed representation of the refresh token for security.
    /// </summary>
    public class RefreshToken
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
        [MaxLength(100)]
        public string? Device { get; set; }

        /// <summary>
        /// IP address from which the token was created.
        /// </summary>
        [MaxLength(45)] // supports IPv6
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
        /// Navigation to ApplicationUser.
        /// </summary>
        public ApplicationUser? User { get; set; }
    }
}
