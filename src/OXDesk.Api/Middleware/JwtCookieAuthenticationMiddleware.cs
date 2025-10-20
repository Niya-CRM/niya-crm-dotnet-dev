using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OXDesk.Api.Helpers;
using OXDesk.Core.Auth.Constants;

namespace OXDesk.Api.Middleware
{
    public class JwtCookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request has the access_token cookie
            if (context.Request.Cookies.TryGetValue("oxd_api_token", out string? token) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    // Validate and parse the JWT token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = JwtHelper.GetJwtSigningKey();
                    
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = AuthConstants.Jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = AuthConstants.Jwt.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    // Validate the token and extract claims
                    var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                    
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
