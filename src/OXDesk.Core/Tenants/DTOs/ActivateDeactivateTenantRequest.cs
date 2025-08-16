using System.ComponentModel.DataAnnotations;

namespace OXDesk.Core.Tenants.DTOs;

/// <summary>
/// Request model for activating a tenant.
/// </summary>
public class ActivateDeactivateTenantRequest
{
    /// <summary>
    /// Gets or sets the reason for activation/deactivation.
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}
