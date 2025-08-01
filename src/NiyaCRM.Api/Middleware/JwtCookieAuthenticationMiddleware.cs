using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NiyaCRM.Api.Middleware
{
    public class JwtCookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtCookieAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request has the access_token cookie
            if (context.Request.Cookies.TryGetValue("access_token", out string? token) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    // Validate and parse the JWT token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "defaultSecretKeyWhichShouldBeReplaced");
                    
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = _configuration["JWT:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = _configuration["JWT:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    // Validate the token and extract claims
                    var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                    
                    // Set the claims principal on the HttpContext
                    context.User = principal;
                }
                catch (Exception)
                {
                    // Token validation failed - don't set the user
                    // Optionally log the exception here
                }
            }

            // Continue processing the request
            await _next(context);
        }
    }

    // Extension method for easy registration in Startup
    public static class JwtCookieAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookieAuthentication(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieAuthenticationMiddleware>();
        }
    }
}
