using Microsoft.AspNetCore.Http;

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
            if (!context.Request.Headers.ContainsKey("Authorization") &&
                context.Request.Cookies.TryGetValue("oxd_api_token", out string? token) &&
                !string.IsNullOrWhiteSpace(token))
            {
                context.Request.Headers.Authorization = $"Bearer {token}";
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
