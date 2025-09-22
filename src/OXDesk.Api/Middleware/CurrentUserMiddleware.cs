using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Identity;

namespace OXDesk.Api.Middleware
{
    /// <summary>
    /// Middleware that populates the ambient current user context from JWT claims when authenticated.
    /// </summary>
    public sealed class CurrentUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CurrentUserMiddleware> _logger;

        public CurrentUserMiddleware(RequestDelegate next, ILogger<CurrentUserMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    // Extract user id from 'sub' or nameidentifier claim (int)
                    var sub = context.User.FindFirst(ClaimTypes.NameIdentifier) ??
                              context.User.FindFirst(JwtRegisteredClaimNamesSub);

                    int? userId = null;
                    if (sub != null && int.TryParse(sub.Value, out var parsed))
                    {
                        userId = parsed;
                    }

                    // Extract roles and permissions
                    var roles = context.User.Claims
                        .Where(c => string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Value)
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToArray();

                    var permissions = context.User.Claims
                        .Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Value)
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToArray();

                    // Extract optional name and email
                    var name = context.User.FindFirst(ClaimTypes.Name)?.Value
                               ?? context.User.FindFirst("name")?.Value;
                    var email = context.User.FindFirst(ClaimTypes.Email)?.Value
                                ?? context.User.FindFirst("email")?.Value;

                    var currentUser = context.RequestServices.GetService(typeof(ICurrentUser)) as ICurrentUser;
                    currentUser?.Change(userId, roles, permissions, name, email);

                    if (userId.HasValue)
                    {
                        _logger.LogDebug("CurrentUser set: {UserId} (Roles: {RoleCount}, Permissions: {PermCount})", userId, roles.Length, permissions.Length);
                    }
                    else
                    {
                        _logger.LogWarning("Authenticated request but could not parse user id (sub claim)");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating CurrentUser from JWT claims");
            }

            await _next(context);
        }

        private static string JwtRegisteredClaimNamesSub => "sub"; // fallback if ClaimTypes.NameIdentifier not present
    }

    public static class CurrentUserMiddlewareExtensions
    {
        public static IApplicationBuilder UseCurrentUserMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CurrentUserMiddleware>();
        }
    }
}
