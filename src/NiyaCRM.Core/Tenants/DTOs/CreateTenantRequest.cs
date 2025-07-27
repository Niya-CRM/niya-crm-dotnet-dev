using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Core.Tenants.DTOs;

/// <summary>
/// Request model for creating a new tenant.
/// </summary>
public class CreateTenantRequest
{
    /// <summary>
    /// Gets or sets the tenant name.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant host.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant time zone.
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string? TimeZone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name (optional).
    /// </summary>
    public string? DatabaseName { get; set; }
}
