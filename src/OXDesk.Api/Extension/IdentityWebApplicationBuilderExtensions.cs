using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OXDesk.Core.Identity;
using OXDesk.DbContext.Data;
using OXDesk.Identity.Extensions;

namespace OXDesk.Api.Extension
{
    /// <summary>
    /// Service registration helpers for configuring ASP.NET Core Identity and related cookie behavior.
    /// </summary>
    public static class IdentityWebApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures ASP.NET Core Identity and cookie redirects.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        /// <param name="cookiePrefix">The cookie prefix to use for Identity cookies.</param>
        public static void ConfigureIdentityAndCookies(this WebApplicationBuilder builder, string cookiePrefix)
        {
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<TenantDbContext>()
            .AddDefaultTokenProviders();

            var frontendBaseUrl = builder.Configuration["FrontEnd"];
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                frontendBaseUrl = "http://localhost:3000";
            }

            frontendBaseUrl = frontendBaseUrl.TrimEnd('/');

            // Configure Identity cookie to redirect to frontend on access denied
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // When not authenticated, redirect to the OAuth authorize endpoint (which shows login)
                // For non-OAuth requests, redirect to the frontend
                options.LoginPath = "/oauth/authorize";
                options.AccessDeniedPath = "/oauth/authorize";

                options.Cookie.Name = $"{cookiePrefix}.Identity";

                // Handle events to redirect to frontend for non-OAuth requests
                options.Events.OnRedirectToLogin = context =>
                {
                    var path = context.Request.Path;

                    // Ignore static file requests (favicon, etc.) - return 401 instead of redirect
                    if (path.StartsWithSegments("/favicon.ico") ||
                        path.Value?.EndsWith(".ico") == true ||
                        path.Value?.EndsWith(".png") == true ||
                        path.Value?.EndsWith(".jpg") == true ||
                        path.Value?.EndsWith(".css") == true ||
                        path.Value?.EndsWith(".js") == true)
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }

                    // If this is an API request, return 401 instead of redirect
                    if (path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }

                    // If this is an OAuth request, continue with normal redirect
                    if (path.StartsWithSegments("/oauth"))
                    {
                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    }

                    // For all other requests, redirect to frontend
                    context.Response.Redirect($"{frontendBaseUrl}/auth/login");
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    // Redirect to frontend for access denied
                    context.Response.Redirect($"{frontendBaseUrl}/auth/login?error=access_denied");
                    return Task.CompletedTask;
                };
            });

            // Configure OpenIddict for OAuth 2.0 and OpenID Connect
            builder.Services.AddOpenIddictServices(builder.Configuration);
        }
    }
}
