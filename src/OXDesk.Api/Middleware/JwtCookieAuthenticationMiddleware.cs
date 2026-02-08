using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace OXDesk.Api.Middleware
{
    public class JwtCookieAuthenticationMiddleware
    {
        private const string CookiePrefix = "oxd_session_jwt_";
        private readonly RequestDelegate _next;

        public JwtCookieAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                var accessToken = GetAccessTokenFromChunkedCookies(context.Request.Cookies);
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    context.Request.Headers.Authorization = $"Bearer {accessToken}";
                }
            }

            // Continue processing the request
            await _next(context);
        }

        private static string? GetAccessTokenFromChunkedCookies(IRequestCookieCollection cookies)
        {
            var chunks = cookies
                .Where(c => c.Key.StartsWith(CookiePrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c =>
                {
                    var suffix = c.Key[CookiePrefix.Length..];
                    return int.TryParse(suffix, out var index) ? index : int.MaxValue;
                })
                .Select(c => c.Value)
                .ToList();

            if (chunks.Count == 0)
                return null;

            var json = string.Concat(chunks);

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("accessToken", out var tokenElement))
                {
                    return tokenElement.GetString();
                }
            }
            catch (JsonException)
            {
                // Malformed cookie payload; ignore
            }

            return null;
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
