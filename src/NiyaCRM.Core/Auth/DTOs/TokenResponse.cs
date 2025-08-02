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
    }
}
