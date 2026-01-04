using System.ComponentModel.DataAnnotations;
using OXDesk.Core.Common.Redaction;

namespace OXDesk.Core.Auth.DTOs;

/// <summary>
/// DTO for OAuth authorization request parameters
/// </summary>
public class AuthorizeRequestDto
{
    /// <summary>
    /// The client identifier
    /// </summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The URI to redirect to after authorization
    /// </summary>
    [Required]
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// The response type (e.g., "code")
    /// </summary>
    [Required]
    public string ResponseType { get; set; } = string.Empty;

    /// <summary>
    /// Space-separated list of scopes
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// State parameter for CSRF protection
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// PKCE code challenge
    /// </summary>
    public string? CodeChallenge { get; set; }

    /// <summary>
    /// PKCE code challenge method (e.g., "S256")
    /// </summary>
    public string? CodeChallengeMethod { get; set; }

    /// <summary>
    /// Tenant identifier for multi-tenant support
    /// </summary>
    public string? Tenant { get; set; }

    /// <summary>
    /// Nonce for OpenID Connect
    /// </summary>
    public string? Nonce { get; set; }
}

/// <summary>
/// DTO for the authorization login form
/// </summary>
public class AuthorizeLoginDto
{
    /// <summary>
    /// User email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [PersonalData]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [SensitiveData]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Remember the user's login
    /// </summary>
    public bool RememberMe { get; set; }

    /// <summary>
    /// The original authorization request parameters (serialized)
    /// </summary>
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// View model for the authorization page
/// </summary>
public class AuthorizeViewModel
{
    /// <summary>
    /// The client application name
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// The requested scopes
    /// </summary>
    public IEnumerable<string> Scopes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The return URL containing authorization parameters
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Login form data
    /// </summary>
    public AuthorizeLoginDto Login { get; set; } = new();
}

/// <summary>
/// DTO for OAuth token request
/// </summary>
public class TokenRequestDto
{
    /// <summary>
    /// The grant type (authorization_code, refresh_token, etc.)
    /// </summary>
    [Required]
    public string GrantType { get; set; } = string.Empty;

    /// <summary>
    /// The authorization code (for authorization_code grant)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// The redirect URI (must match the one used in authorization)
    /// </summary>
    public string? RedirectUri { get; set; }

    /// <summary>
    /// The client identifier
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// The client secret (for confidential clients)
    /// </summary>
    [SensitiveData]
    public string? ClientSecret { get; set; }

    /// <summary>
    /// PKCE code verifier
    /// </summary>
    public string? CodeVerifier { get; set; }

    /// <summary>
    /// The refresh token (for refresh_token grant)
    /// </summary>
    [SensitiveData]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Space-separated list of scopes
    /// </summary>
    public string? Scope { get; set; }
}

/// <summary>
/// OAuth token response
/// </summary>
public class OAuthTokenResponse
{
    /// <summary>
    /// The access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The token type (typically "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The refresh token
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Space-separated list of granted scopes
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// The ID token (for OpenID Connect)
    /// </summary>
    public string? IdToken { get; set; }
}

/// <summary>
/// OAuth error response
/// </summary>
public class OAuthErrorResponse
{
    /// <summary>
    /// Error code
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error description
    /// </summary>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// URI for more information about the error
    /// </summary>
    public string? ErrorUri { get; set; }
}
