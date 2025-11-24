using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.Identity;

namespace OXDesk.Identity.Controllers;

/// <summary>
/// Controller for handling OpenIddict OAuth 2.0 and OpenID Connect operations
/// </summary>
[Route("api/auth")]
[ApiController]
public class OpenIddictAuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAuditLogService _auditLogService;
    private readonly IDynamicObjectService _dynamicObjectService;
    private readonly ILogger<OpenIddictAuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenIddictAuthController"/> class.
    /// </summary>
    public OpenIddictAuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IAuditLogService auditLogService,
        IDynamicObjectService dynamicObjectService,
        ILogger<OpenIddictAuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _auditLogService = auditLogService;
        _dynamicObjectService = dynamicObjectService;
        _logger = logger;
    }

    /// <summary>
    /// Handles token requests (password and refresh_token grant types)
    /// </summary>
    /// <returns>Token response</returns>
    /// <response code="200">Returns the access token and refresh token</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="401">If authentication fails</response>
    [HttpPost("token")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidRequest,
                ErrorDescription = "The OpenID Connect request cannot be retrieved."
            });
        }

        if (request.IsPasswordGrantType())
        {
            return await HandlePasswordGrantAsync(request);
        }

        if (request.IsRefreshTokenGrantType())
        {
            return await HandleRefreshTokenGrantAsync(request);
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = OpenIddictConstants.Errors.UnsupportedGrantType,
            ErrorDescription = "The specified grant type is not supported."
        });
    }

    /// <summary>
    /// Handles password grant type (Resource Owner Password Credentials)
    /// </summary>
    private async Task<IActionResult> HandlePasswordGrantAsync(OpenIddictRequest request)
    {
        _logger.LogInformation("Processing password grant for user: {Username}", request.Username);

        // Validate username and password
        var user = await _userManager.FindByEmailAsync(request.Username ?? string.Empty);
        if (user == null)
        {
            _logger.LogWarning("User not found: {Username}", request.Username);
            return Unauthorized(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "Invalid email or password."
            });
        }

        var userObjectId = await _dynamicObjectService.GetDynamicObjectIdAsync(
            DynamicObjectConstants.DynamicObjectKeys.User);

        // Check if user is active
        if (user.IsActive != "Y")
        {
            _logger.LogWarning("User account is deactivated: {Email}", user.Email);
            
            // Audit: Login denied due to inactive account
            await _auditLogService.CreateAuditLogAsync(
                objectId: userObjectId,
                @event: CommonConstant.AUDIT_LOG_EVENT_LOGIN,
                objectItemId: user.Id,
                ip: HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                data: "Login Denied - Account not Active",
                createdBy: user.Id,
                cancellationToken: default
            );

            return Unauthorized(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "Account is deactivated. Please contact Support."
            });
        }

        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password ?? string.Empty, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Invalid password for user: {Email}", user.Email);
            
            // Audit: Invalid credential attempt
            await _auditLogService.CreateAuditLogAsync(
                objectId: userObjectId,
                @event: CommonConstant.AUDIT_LOG_EVENT_LOGIN,
                objectItemId: user.Id,
                ip: HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                data: "Invalid Credential",
                createdBy: user.Id,
                cancellationToken: default
            );

            return Unauthorized(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "Invalid email or password."
            });
        }

        _logger.LogInformation("User authenticated successfully: {Email}", user.Email);
        
        // Audit: Successful login
        await _auditLogService.CreateAuditLogAsync(
            objectId: userObjectId,
            @event: CommonConstant.AUDIT_LOG_EVENT_LOGIN,
            objectItemId: user.Id,
            ip: HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
            data: "Login Successful",
            createdBy: user.Id,
            cancellationToken: default
        );

        // Create claims principal
        var principal = await CreateClaimsPrincipalAsync(user);

        // Set the list of scopes granted to the client application
        principal.SetScopes(new[]
        {
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Roles,
            "api"
        }.Intersect(request.GetScopes()));

        // Set destinations for claims (access_token, id_token, or both)
        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim));
        }

        // Sign in and return tokens
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handles refresh token grant type
    /// </summary>
    private async Task<IActionResult> HandleRefreshTokenGrantAsync(OpenIddictRequest request)
    {
        _logger.LogInformation("Processing refresh token grant");

        // Retrieve the claims principal stored in the refresh token
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result?.Principal == null)
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "The refresh token is no longer valid."
            });
        }

        // Retrieve the user profile corresponding to the refresh token
        var userId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "The refresh token is no longer valid."
            });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsActive != "Y")
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "The refresh token is no longer valid."
            });
        }

        // Create a new claims principal
        var principal = await CreateClaimsPrincipalAsync(user);

        // Restore the scopes from the original refresh token
        principal.SetScopes(result.Principal.GetScopes());

        // Set destinations for claims
        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim));
        }

        // Sign in and return new tokens
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Creates a claims principal for the user with roles and permissions
    /// </summary>
    private async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user)
    {
        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        // Add standard claims
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? string.Empty));

        // Add tenant_id claim
        identity.AddClaim(new Claim("tenant_id", user.TenantId.ToString()));

        // Add user profile
        if (!string.IsNullOrEmpty(user.Profile))
        {
            identity.AddClaim(new Claim("profile", user.Profile));
        }

        // Add role claims
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
            identity.AddClaim(new Claim("role", role));
        }

        // Add permission claims aggregated from roles
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
            identity.AddClaim(new Claim("permission", perm));
        }

        var principal = new ClaimsPrincipal(identity);

        // Set resource (audience)
        principal.SetResources("oxdesk-api");

        return principal;
    }

    /// <summary>
    /// Determines the destinations (access_token, id_token) for each claim
    /// </summary>
    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination.

        switch (claim.Type)
        {
            case ClaimTypes.Name:
            case ClaimTypes.Email:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield return OpenIddictConstants.Destinations.IdentityToken;
                break;

            case ClaimTypes.NameIdentifier:
            case "tenant_id":
            case "profile":
                yield return OpenIddictConstants.Destinations.AccessToken;
                break;

            case ClaimTypes.Role:
            case "role":
            case "permission":
                yield return OpenIddictConstants.Destinations.AccessToken;
                break;

            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                break;
        }
    }

    /// <summary>
    /// Logs out the current user by revoking tokens
    /// </summary>
    /// <returns>No content on success</returns>
    /// <response code="204">Tokens revoked successfully</response>
    /// <response code="401">If not authenticated</response>
    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        // Sign out from ASP.NET Core Identity
        await _signInManager.SignOutAsync();

        // Return success
        return NoContent();
    }
}
