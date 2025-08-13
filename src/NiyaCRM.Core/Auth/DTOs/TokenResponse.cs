namespace NiyaCRM.Core.Auth.DTOs
{
    /// <summary>
    /// Response model for authentication token
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// JWT token string
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// Token expiration time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Type of token (e.g. "Bearer")
        /// </summary>
        public required string TokenType { get; set; }

        /// <summary>
        /// The refresh token issued alongside the access token.
        /// </summary>
        public required string RefreshToken { get; set; }

        /// <summary>
        /// The unique identifier of the authenticated user
        /// </summary>
        public System.Guid Id { get; set; }

        /// <summary>
        /// Display name of the user (e.g., FirstName LastName)
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Email address of the user
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Roles assigned to the user
        /// </summary>
        public System.Collections.Generic.IEnumerable<string> Roles { get; set; } = System.Array.Empty<string>();
    }
}
