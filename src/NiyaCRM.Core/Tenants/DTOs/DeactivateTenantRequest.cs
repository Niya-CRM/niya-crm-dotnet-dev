using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Core.Tenants.DTOs;

/// <summary>
/// Request model for deactivating a tenant.
/// </summary>
public class DeactivateTenantRequest
{
    /// <summary>
    /// Gets or sets the user deactivating the tenant.
    /// </summary>
    [StringLength(100)]
    public string? ModifiedBy { get; set; }
}
