using System.ComponentModel.DataAnnotations;

namespace OXDesk.Core.Identity.DTOs;

/// <summary>
/// Request model for activating or deactivating a user.
/// </summary>
public class ActivateDeactivateUserRequest
{
    /// <summary>
    /// Gets or sets the reason for activation/deactivation.
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}
