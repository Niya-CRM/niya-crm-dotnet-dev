using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Core.Tenants.DTOs;

/// <summary>
/// Request model for updating an existing tenant.
/// </summary>
public class UpdateTenantRequest
{
    /// <summary>
    /// Gets or sets the tenant name.
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant host.
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string? Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant email.
    /// </summary>
    [EmailAddress]
    [StringLength(100, MinimumLength = 1)]
    public string? Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name (optional).
    /// </summary>
    [StringLength(100)]
    public string? DatabaseName { get; set; }
}
