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
                // Enable the token endpoint for password and refresh_token flows
                options.SetTokenEndpointUris("/api/auth/token");

                // Enable the password flow (Resource Owner Password Credentials)
                options.AllowPasswordFlow();
                
                // Enable the refresh token flow
                options.AllowRefreshTokenFlow();

                // Accept anonymous clients (i.e., clients that don't send a client_id)
                options.AcceptAnonymousClients();

                // Register signing and encryption credentials
                // In production, use certificates or keys from secure storage
                var signingKey = GetSigningKey();
                options.AddSigningKey(signingKey);
                options.AddEncryptionKey(signingKey);

                // Register scopes (permissions)
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    "api"
                );

                // Set token lifetimes
                options.SetAccessTokenLifetime(TimeSpan.FromHours(AuthConstants.Jwt.TokenExpiryHours));
                options.SetRefreshTokenLifetime(TimeSpan.FromHours(AuthConstants.Refresh.RefreshTokenExpiryHours));

                // Configure refresh token behavior
                // Allow refresh token reuse within a 10-minute window to handle network delays
                options.SetRefreshTokenReuseLeeway(TimeSpan.FromMinutes(10));

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options
                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
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
    /// Gets the signing key for OpenIddict from environment variables
    /// </summary>
    /// <returns>Symmetric security key</returns>
    private static Microsoft.IdentityModel.Tokens.SymmetricSecurityKey GetSigningKey()
    {
        string? jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        
        if (string.IsNullOrEmpty(jwtSecret))
        {
            // Check if we're in development environment
            string? environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            bool isDevelopment = string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase);
            
            if (isDevelopment)
            {
                // Generate a random key for development
                jwtSecret = GenerateRandomKey(64);
                Console.WriteLine("WARNING: JWT_SECRET environment variable not found. Using a randomly generated key for development.");
                Console.WriteLine($"Generated key: {jwtSecret}");
            }
            else
            {
                throw new InvalidOperationException("JWT_SECRET environment variable not found");
            }
        }
        
        return new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSecret));
    }

    /// <summary>
    /// Generates a random key with the specified length
    /// </summary>
    /// <param name="length">The length of the key to generate</param>
    /// <returns>A random key</returns>
    private static string GenerateRandomKey(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
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

        // Check if the application already exists
        if (await manager.FindByClientIdAsync("oxdesk-web-client") == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "oxdesk-web-client",
                DisplayName = "OXDesk Web Client",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api"
                }
            });
        }
    }
}
