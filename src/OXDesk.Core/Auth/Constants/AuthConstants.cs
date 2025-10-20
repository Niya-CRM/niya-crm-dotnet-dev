namespace OXDesk.Core.Auth.Constants
{
    public static class AuthConstants
    {
        // JWT Settings
        public static class Jwt
        {
            public const double TokenExpiryHours = 0.15; //10 minutes
            public const string SecretConfigKey = "JWT:Secret";
            public const string Issuer = "OXDesk";
            public const string Audience = "OXDeskClient";
            public const string SecurityAlgorithm = "HmacSha256";
            public const string TokenType = "Bearer";
        }

        // Refresh Token Settings
        public static class Refresh
        {
            // Time-to-live for refresh tokens in hours
            public const double RefreshTokenExpiryHours = 4.0; // 4 Hours
        }

        // Cookie Settings
        public static class Cookie
        {
            public const string AccessTokenName = "access_token";
            public const int ExpiryHours = 8; // 8 Hours
            public const string SameSiteMode = "Strict";
        }
        
        // Session Settings
        public static class Session
        {
            public const int ExpiryHours = 8; // 8 Hours
        }
    }
}
