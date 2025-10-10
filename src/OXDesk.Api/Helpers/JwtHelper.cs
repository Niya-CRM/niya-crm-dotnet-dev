using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OXDesk.Core.Auth.Constants;
using OXDesk.Core.Auth.DTOs;
using OXDesk.Core.Identity;
using System.Linq;

namespace OXDesk.Api.Helpers
{
    /// <summary>
    /// Helper class for JWT token generation, validation, and refresh token management.
    /// </summary>
    public class JwtHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUserRefreshTokenRepository _refreshTokenRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        // Static field to store the generated random key for development
        private static byte[]? _devSigningKey = null;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtHelper"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        /// <param name="refreshTokenRepository">The refresh token repository.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public JwtHelper(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUserRefreshTokenRepository refreshTokenRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
        /// Generates a cryptographically secure refresh token string.
        /// </summary>
        private static string GenerateSecureRefreshToken(int size = 64)
        {
            var bytes = RandomNumberGenerator.GetBytes(size);
            return Base64UrlEncode(bytes);
        }

        /// <summary>
        /// Encodes bytes to Base64 URL-safe format.
        /// </summary>
        /// <param name="bytes">The bytes to encode.</param>
        /// <returns>Base64 URL-safe encoded string.</returns>
        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        /// <summary>
        /// Computes SHA256 hash of input and returns Base64 encoded string.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <returns>Base64 encoded SHA256 hash.</returns>
        private static string ComputeSha256Base64(string input)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Gets the client IP address from the HTTP context.
        /// </summary>
        /// <returns>The client IP address or null.</returns>
        private string? GetClientIp()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null) return null;

            var fwd = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(fwd))
            {
                var ip = fwd.Split(',')[0].Trim();
                return ip.Length > 45 ? ip[..45] : ip;
            }

            var remote = ctx.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(remote)) return null;
            return remote.Length > 45 ? remote[..45] : remote;
        }

        /// <summary>
        /// Gets the client device information from User-Agent header.
        /// </summary>
        /// <returns>The client device string or null.</returns>
        private string? GetClientDevice()
        {
            var ctx = _httpContextAccessor.HttpContext;
            var ua = ctx?.Request?.Headers.UserAgent.ToString();
            if (string.IsNullOrWhiteSpace(ua)) return null;
            return ua.Length > 200 ? ua[..200] : ua;
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
                new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
            };

            // Add tenant_id claim if the user has a tenant assigned
            claims.Add(new Claim("tenant_id", user.TenantId.ToString()));

            // Add role claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim("role", role));
            }

            // Add permission claims aggregated from role claims (claim type: "permission")
            var permissionValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var roleName in userRoles)
            {
                var roleEntity = await _roleManager.FindByNameAsync(roleName);
                if (roleEntity == null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                foreach (var rc in roleClaims)
                {
                    if (string.Equals(rc.Type, "permission", StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(rc.Value))
                    {
                        permissionValues.Add(rc.Value);
                    }
                }
            }

            foreach (var perm in permissionValues)
            {
                claims.Add(new Claim("permission", perm));
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

        /// <summary>
        /// Builds a TokenResponse containing the JWT and user info (Id, Name, Email, Roles).
        /// </summary>
        /// <param name="user">Authenticated user</param>
        /// <returns>A populated TokenResponse</returns>
        public async Task<TokenResponse> BuildTokenResponse(ApplicationUser user)
        {
            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            // Generate and persist refresh token (store only hash)
            var refreshTokenRaw = GenerateSecureRefreshToken(64);
            var refreshTokenHash = ComputeSha256Base64(refreshTokenRaw);

            var refreshEntity = new UserRefreshToken
            {
                Id = Guid.CreateVersion7(),
                UserId = user.Id,
                HashedToken = refreshTokenHash,
                Device = GetClientDevice(),
                IpAddress = GetClientIp(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(AuthConstants.Refresh.RefreshTokenExpiryHours)
            };
            await _refreshTokenRepository.AddAsync(refreshEntity);

            var name = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (string.IsNullOrWhiteSpace(name))
            {
                name = user.UserName ?? user.Email ?? string.Empty;
            }

            return new TokenResponse
            {
                Token = token,
                ExpiresIn = (int)(AuthConstants.Jwt.TokenExpiryHours * 3600),
                TokenType = AuthConstants.Jwt.TokenType,
                RefreshToken = refreshTokenRaw,
                Id = user.Id,
                Name = name,
                Email = user.Email ?? string.Empty,
                Roles = roles
            };
        }

        /// <summary>
        /// Validates a refresh token and issues a new access and refresh token pair.
        /// Marks the token's first use timestamp (UsedAt) and permits reuse within 10 minutes
        /// without re-updating UsedAt. Does not delete used tokens.
        /// Returns null if invalid, expired, or beyond the 10-minute reuse window.
        /// </summary>
        public async Task<TokenResponse?> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            var hashed = ComputeSha256Base64(refreshToken);
            var existing = await _refreshTokenRepository.GetByHashedTokenAsync(hashed);
            if (existing == null)
            {
                return null;
            }

            // Expiry check
            if (existing.ExpiresAt <= DateTime.UtcNow)
            {
                await _refreshTokenRepository.DeleteByHashedTokenAsync(hashed);
                return null;
            }

            // Enforce single-use with 10-minute grace window (do not re-update UsedAt)
            if (existing.UsedAt == null)
            {
                existing.UsedAt = DateTime.UtcNow;
                existing.UsedCounter += 1;
                await _refreshTokenRepository.UpdateAsync(existing);
            }
            else
            {
                var sinceFirstUse = DateTime.UtcNow - existing.UsedAt.Value;
                if (sinceFirstUse > TimeSpan.FromMinutes(10))
                {
                    return null;
                }
                // Within grace window: increment counter but do not update UsedAt
                existing.UsedCounter += 1;
                await _refreshTokenRepository.UpdateAsync(existing);
            }

            // Load user and ensure active
            var user = await _userManager.FindByIdAsync(existing.UserId.ToString());
            if (user == null || user.IsActive != "Y")
            {
                // Cleanup the stale token
                await _refreshTokenRepository.DeleteByHashedTokenAsync(hashed);
                return null;
            }

            // Build a new token response which also issues a fresh refresh token
            return await BuildTokenResponse(user);
        }
    }
}
