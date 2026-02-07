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
using OXDesk.Core.Auth.DTOs;
using OXDesk.Core.Common;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.Identity;
using OXDesk.Core.Tenants;

namespace OXDesk.Identity.Controllers;

/// <summary>
/// Controller for handling OAuth 2.0 Authorization Code Flow with PKCE
/// </summary>
[Route("oauth")]
public class OAuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IAuditLogService _auditLogService;
    private readonly IDynamicObjectService _dynamicObjectService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<OAuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthController"/> class.
    /// </summary>
    public OAuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        IAuditLogService auditLogService,
        IDynamicObjectService dynamicObjectService,
        ITenantService tenantService,
        ILogger<OAuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _auditLogService = auditLogService;
        _dynamicObjectService = dynamicObjectService;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the authorization endpoint - displays login page (GET) or processes login/consent (POST)
    /// </summary>
    /// <returns>Login view or redirect with authorization code</returns>
    [HttpGet("authorize")]
    [AllowAnonymous]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        _logger.LogInformation("Authorization GET request received for client: {ClientId}", request.ClientId);

        // Validate the client application
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty);
        if (application == null)
        {
            _logger.LogWarning("Unknown client: {ClientId}", request.ClientId);
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidClient,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified client is not registered."
                }));
        }

        // If the user is already authenticated, issue the authorization code directly
        if (User.Identity?.IsAuthenticated == true)
        {
            return await IssueAuthorizationCodeAsync(request);
        }

        // Build the return URL with all OAuth parameters for the login form
        var returnUrl = Request.PathBase + Request.Path + QueryString.Create(Request.Query.ToList());

        var viewModel = new AuthorizeViewModel
        {
            ApplicationName = await _applicationManager.GetDisplayNameAsync(application),
            Scopes = request.GetScopes(),
            ReturnUrl = returnUrl,
            Login = new AuthorizeLoginDto { ReturnUrl = returnUrl }
        };

        return View("Authorize", viewModel);
    }

    /// <summary>
    /// Handles the authorization POST - processes login form submission
    /// </summary>
    /// <returns>Login view with error or redirect with authorization code</returns>
    [HttpPost("authorize")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AuthorizePost([Bind(Prefix = "Login")] AuthorizeLoginDto model)
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        _logger.LogInformation("Authorization POST request received for client: {ClientId}", request.ClientId);

        // Validate the client application
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty);
        if (application == null)
        {
            _logger.LogWarning("Unknown client: {ClientId}", request.ClientId);
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidClient,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified client is not registered."
                }));
        }

        // If the user is already authenticated, issue the authorization code directly
        if (User.Identity?.IsAuthenticated == true)
        {
            return await IssueAuthorizationCodeAsync(request);
        }

        // Build the return URL for error cases
        var returnUrl = Request.PathBase + Request.Path + QueryString.Create(Request.Query.ToList());

        // Validate model
        if (!ModelState.IsValid)
        {
            var viewModel = new AuthorizeViewModel
            {
                ApplicationName = await _applicationManager.GetDisplayNameAsync(application),
                Scopes = request.GetScopes(),
                ReturnUrl = returnUrl,
                Login = model
            };
            return View("Authorize", viewModel);
        }

        _logger.LogInformation("Processing login for OAuth authorization: {Email}", model.Email);

        // Find the user
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            _logger.LogWarning("User not found: {Email}", model.Email);
            return await ReturnLoginErrorAsync(request, application, model, returnUrl, "Invalid email or password.");
        }

        // Check if user is active
        if (user.IsActive != "Y")
        {
            _logger.LogWarning("User account is deactivated: {Email}", user.Email);
            
            var userObjectId = await _dynamicObjectService.GetDynamicObjectIdAsync(DynamicObjectConstants.DynamicObjectKeys.User);
            await _auditLogService.CreateAuditLogAsync(
                @event: CommonConstant.AUDIT_LOG_EVENT_LOGIN,
                objectId: userObjectId,
                objectItemId: user.Id,
                ip: HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                data: "OAuth Login Denied - Account not Active",
                createdBy: user.Id,
                cancellationToken: default
            );

            return await ReturnLoginErrorAsync(request, application, model, returnUrl, "Account is deactivated. Please contact Support.");
        }

        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Invalid password for user: {Email}", user.Email);
            
            var userObjectId = await _dynamicObjectService.GetDynamicObjectIdAsync(DynamicObjectConstants.DynamicObjectKeys.User);
            await _auditLogService.CreateAuditLogAsync(
                @event: CommonConstant.AUDIT_LOG_EVENT_LOGIN,
                objectId: userObjectId,
                objectItemId: user.Id,
                ip: HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                data: "OAuth Invalid Credential",
                createdBy: user.Id,
                cancellationToken: default
            );

            return await ReturnLoginErrorAsync(request, application, model, returnUrl, "Invalid email or password.");
        }

        _logger.LogInformation("User authenticated successfully for OAuth: {Email}", user.Email);

        // Audit: Successful login
        var userObjId = await _dynamicObjectService.GetDynamicObjectIdAsync(DynamicObjectConstants.DynamicObjectKeys.User);
        await _auditLogService.CreateAuditLogAsync(
            @event: CommonConstant.AUDIT_LOG_EVENT_LOGIN,
            objectId: userObjId,
            objectItemId: user.Id,
            ip: HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
            data: "OAuth Login Successful",
            createdBy: user.Id,
            cancellationToken: default
        );

        // Sign in the user with ASP.NET Identity (sets the cookie)
        await _signInManager.SignInAsync(user, model.RememberMe);

        // Now issue the authorization code
        var principal = await CreateClaimsPrincipalAsync(user, request);
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Issues an authorization code for an already authenticated user
    /// </summary>
    private async Task<IActionResult> IssueAuthorizationCodeAsync(OpenIddictRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out _))
        {
            // User claim is invalid, challenge to re-authenticate
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(Request.Query.ToList())
                });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(Request.Query.ToList())
                });
        }

        // Create the claims principal for the authorization code
        var principal = await CreateClaimsPrincipalAsync(user, request);

        // Sign in and return the authorization code
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }


    /// <summary>
    /// Handles token requests (authorization_code and refresh_token grant types)
    /// </summary>
    /// <returns>Token response</returns>
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

        if (request.IsAuthorizationCodeGrantType())
        {
            return await HandleAuthorizationCodeGrantAsync();
        }

        if (request.IsRefreshTokenGrantType())
        {
            return await HandleRefreshTokenGrantAsync();
        }

        _logger.LogWarning("Unsupported grant type: {GrantType}", request.GrantType);
        return BadRequest(new OpenIddictResponse
        {
            Error = OpenIddictConstants.Errors.UnsupportedGrantType,
            ErrorDescription = "The specified grant type is not supported."
        });
    }

    /// <summary>
    /// Handles logout requests
    /// </summary>
    [HttpGet("logout")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Sign out from ASP.NET Core Identity
        await _signInManager.SignOutAsync();

        // Redirect to the post-logout redirect URI if specified
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    /// <summary>
    /// Handles authorization code grant type
    /// </summary>
    private async Task<IActionResult> HandleAuthorizationCodeGrantAsync()
    {
        _logger.LogInformation("Processing authorization code grant");

        // Retrieve the claims principal stored in the authorization code
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result?.Principal == null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The authorization code is no longer valid."
                }));
        }

        // Retrieve the user profile
        var userId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The authorization code is no longer valid."
                }));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsActive != "Y")
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user associated with this authorization code is no longer valid."
                }));
        }

        // Create a new claims principal with fresh claims
        var request = HttpContext.GetOpenIddictServerRequest()!;
        var principal = await CreateClaimsPrincipalAsync(user, request);

        // Restore the scopes from the original authorization code
        principal.SetScopes(result.Principal.GetScopes());

        _logger.LogInformation("Authorization code exchanged successfully for user: {Email}", user.Email);

        // Sign in and return tokens
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handles refresh token grant type
    /// </summary>
    private async Task<IActionResult> HandleRefreshTokenGrantAsync()
    {
        _logger.LogInformation("Processing refresh token grant");

        // Retrieve the claims principal stored in the refresh token
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result?.Principal == null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                }));
        }

        // Retrieve the user profile corresponding to the refresh token
        var userId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
                }));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsActive != "Y")
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user associated with this refresh token is no longer valid."
                }));
        }

        // Create a new claims principal with fresh claims
        var request = HttpContext.GetOpenIddictServerRequest()!;
        var principal = await CreateClaimsPrincipalAsync(user, request);

        // Restore the scopes from the original refresh token
        principal.SetScopes(result.Principal.GetScopes());

        _logger.LogInformation("Refresh token exchanged successfully for user: {Email}", user.Email);

        // Sign in and return new tokens
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Creates a claims principal for the user with roles and permissions
    /// </summary>
    private async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user, OpenIddictRequest request)
    {
        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);

        // Add subject claim (required by OpenIddict)
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString())
            .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));

        // Add standard claims
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
        
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
            .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));
        
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            .SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken));

        // Add tenant_id claim
        identity.AddClaim(new Claim("tenant_id", user.TenantId.ToString())
            .SetDestinations(OpenIddictConstants.Destinations.AccessToken));

        // Add user profile
        if (!string.IsNullOrEmpty(user.Profile))
        {
            identity.AddClaim(new Claim("profile", user.Profile)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
        }

        // Add role claims
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
            identity.AddClaim(new Claim("role", role)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
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
            identity.AddClaim(new Claim("permission", perm)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
        }

        var principal = new ClaimsPrincipal(identity);

        // Set the list of scopes granted to the client application
        principal.SetScopes(request.GetScopes());

        // Set resource (audience)
        principal.SetResources("oxdesk-api");

        return principal;
    }

    /// <summary>
    /// Returns to the login view with an error message
    /// </summary>
    private async Task<IActionResult> ReturnLoginErrorAsync(
        OpenIddictRequest request,
        object application,
        AuthorizeLoginDto model,
        string returnUrl,
        string error)
    {
        var viewModel = new AuthorizeViewModel
        {
            ApplicationName = await _applicationManager.GetDisplayNameAsync(application),
            Scopes = request.GetScopes().ToList(),
            ReturnUrl = returnUrl,
            Error = error,
            Login = model
        };

        return View("Authorize", viewModel);
    }
}
