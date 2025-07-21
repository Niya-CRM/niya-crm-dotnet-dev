using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Core.Tenants.DTOs;

/// <summary>
/// Request model for activating a tenant.
/// </summary>
public class ActivateTenantRequest
{
    /// <summary>
    /// Gets or sets the user activating the tenant.
    /// </summary>
    [StringLength(100)]
    public string? ModifiedBy { get; set; }
}
