using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OXDesk.Core.Auth.Constants;
using OXDesk.DbContext.Data;

namespace OXDesk.Identity.Extensions;

/// <summary>
/// Extension methods for configuring OpenIddict services
/// </summary>
public static class OpenIddictExtensions
{
    /// <summary>
    /// Adds and configures OpenIddict services for OAuth 2.0 and OpenID Connect
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOpenIddictServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOpenIddict()
            // Register the OpenIddict core components
            .AddCore(options =>
            {
                // Configure OpenIddict to use Entity Framework Core stores and models
                options.UseEntityFrameworkCore()
                    .UseDbContext<TenantDbContext>();
            })
            // Register the OpenIddict server components
            .AddServer(options =>
            {
                // Set issuer to match local development domain
                options.SetIssuer(new Uri("http://oxdesk.local/"));

                // Enable the authorization endpoint for authorization code flow
                options.SetAuthorizationEndpointUris("/oauth/authorize");
                
                // Enable the token endpoint
                options.SetTokenEndpointUris("/oauth/token");
                
                // Enable the logout endpoint
                options.SetLogoutEndpointUris("/oauth/logout");

                // Enable the authorization code flow with PKCE (required for public clients like SPAs)
                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();
                
                // Enable the refresh token flow
                options.AllowRefreshTokenFlow();

                // Register signing and encryption credentials
                // In development, use ephemeral keys (auto-generated on each startup)
                // In production, use certificates from secure storage
                string? environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool isDevelopment = string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase);

                if (isDevelopment)
                {
                    // Use development certificates (persisted to disk, auto-generated if missing)
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();
                }
                else
                {
                    // In production, load certificates from secure storage
                    // TODO: Configure production certificates
                    // options.AddEncryptionCertificate(certificate);
                    // options.AddSigningCertificate(certificate);
                    
                    // Fallback to development certificates for now
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();
                }

                // Register scopes (permissions)
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    "api"
                );

                // Issue signed JWTs (JWS) instead of encrypted tokens (JWE)
                options.DisableAccessTokenEncryption();
                options.UseReferenceRefreshTokens();

                // Set token lifetimes
                options.SetAccessTokenLifetime(TimeSpan.FromHours(AuthConstants.Jwt.TokenExpiryHours));
                options.SetRefreshTokenLifetime(TimeSpan.FromHours(AuthConstants.Refresh.RefreshTokenExpiryHours));
                options.SetAuthorizationCodeLifetime(TimeSpan.FromMinutes(5));

                // Configure refresh token behavior
                // Allow refresh token reuse within a 2-minute window to handle network delays
                options.SetRefreshTokenReuseLeeway(TimeSpan.FromMinutes(2));

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .DisableTransportSecurityRequirement(); // Only for development, remove in production
            })
            // Register the OpenIddict validation components
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance
                options.UseLocalServer();

                // Register the ASP.NET Core host
                options.UseAspNetCore();
            });

        return services;
    }

    /// <summary>
    /// Seeds the OpenIddict application configuration
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>Task</returns>
    public static async Task SeedOpenIddictApplicationsAsync(this Microsoft.AspNetCore.Builder.WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed NextJS client for authorization code flow with PKCE
        if (await manager.FindByClientIdAsync("oxdesk-web") == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "oxdesk-web",
                DisplayName = "OXDesk Frontend",
                ClientType = OpenIddictConstants.ClientTypes.Public,
                RedirectUris =
                {
                    new Uri("https://oxdesk.local/auth/callback"),
                    new Uri("http://localhost:3000/auth/callback"),
                    new Uri("https://localhost:3000/auth/callback")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("https://oxdesk.local/"),
                    new Uri("http://localhost:3000/"),
                    new Uri("https://localhost:3000/")
                },
                Permissions =
                {
                    // Endpoints
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Logout,
                    
                    // Grant types
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    
                    // Response types
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    
                    // Scopes
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api"
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            });
        }
    }
}
