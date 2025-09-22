using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OXDesk.Core.Identity;

/// <summary>
/// Custom application user role that uses Guid as primary key with audit fields.
/// </summary>
public class ApplicationUserRole : IdentityUserRole<int>
{
    public ApplicationUserRole() : base() 
    { 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.Column("tenant_id")]
    [Required]
    public int TenantId { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who created this user role.
    /// </summary>
    [Required]
    public int CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this user role was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who last updated this user role.
    /// </summary>
    [Required]
    public int UpdatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this user role was last updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }
}
