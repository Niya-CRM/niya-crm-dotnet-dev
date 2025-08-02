using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NiyaCRM.Core.Auth.Constants;
using NiyaCRM.Core.Identity;

namespace NiyaCRM.Api.Helpers
{
    public class JwtHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        
        // Static field to store the generated random key for development
        private static byte[]? _devSigningKey = null;
        private static readonly object _lockObject = new object();

        public JwtHelper(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        /// <summary>
        /// Gets the JWT signing key from environment variables.
        /// In development environment, generates a random key if the environment variable is not set.
        /// </summary>
        /// <returns>JWT signing key as byte array</returns>
        public static byte[] GetJwtSigningKey()
        {
            string? jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
            
            if (string.IsNullOrEmpty(jwtSecret))
            {
                // Check if we're in development environment
                string? environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool isDevelopment = string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase);
                
                if (isDevelopment)
                {
                    // Check if we already have a dev signing key cached
                    if (_devSigningKey != null)
                    {
                        return _devSigningKey;
                    }
                    
                    // Use double-check locking pattern to ensure thread safety
                    if (_devSigningKey == null)
                    {
                        lock (_lockObject)
                        {
                            if (_devSigningKey == null)
                            {
                                // Generate a random 64-character key for development
                                jwtSecret = GenerateRandomKey(64);
                                _devSigningKey = Encoding.UTF8.GetBytes(jwtSecret);
                                Console.WriteLine("WARNING: JWT_SECRET environment variable not found. Using a randomly generated key for development.");
                                Console.WriteLine($"Generated key: {jwtSecret}");
                            }
                        }
                    }
                    
                    return _devSigningKey!;
                }
                else
                {
                    throw new InvalidOperationException("JWT_SECRET environment variable not found");
                }
            }
            
            return Encoding.UTF8.GetBytes(jwtSecret);
        }
        
        /// <summary>
        /// Generates a random key with the specified length
        /// </summary>
        /// <param name="length">The length of the key to generate</param>
        /// <returns>A random key</returns>
        private static string GenerateRandomKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"; // URL-safe Base64 chars
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a JWT token for the given user
        /// </summary>
        /// <param name="user">The user to generate the token for</param>
        /// <returns>The generated JWT token</returns>
        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim("role", role));
            }

            var key = new SymmetricSecurityKey(GetJwtSigningKey());
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(AuthConstants.Jwt.TokenExpiryHours);

            var token = new JwtSecurityToken(
                issuer: AuthConstants.Jwt.Issuer,
                audience: AuthConstants.Jwt.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
