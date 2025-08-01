namespace NiyaCRM.Core.Auth.Constants
{
    public static class AuthConstants
    {
        // JWT Settings
        public static class Jwt
        {
            public const int TokenExpiryHours = 1;
            public const string SecretConfigKey = "JWT:Secret";
            public const string Issuer = "NiyaCRM";
            public const string Audience = "NiyaCRMClient";
            public const string SecurityAlgorithm = "HmacSha256";
            public const string TokenType = "Bearer";
        }

        // Cookie Settings
        public static class Cookie
        {
            public const string AccessTokenName = "access_token";
            public const int ExpiryHours = 9;
            public const string SameSiteMode = "Strict";
        }
    }
}
