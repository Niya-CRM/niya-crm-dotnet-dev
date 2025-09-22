using System.ComponentModel.DataAnnotations;

namespace OXDesk.Core.Tenants.DTOs;

/// <summary>
/// Request model for creating a new tenant.
/// </summary>
public class CreateTenantRequest
{
    /// <summary>
    /// Gets or sets the tenant name.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant host.
    /// </summary>
    [Required]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the tenant email.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant time zone.
    /// </summary>
    [Required]
    public string? TimeZone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name (optional).
    /// </summary>
    [Required]
    public string DatabaseName { get; set; } = string.Empty;
}
