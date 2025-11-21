using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Identity;

/// <summary>
/// Custom application role claim that uses int as primary key with audit fields.
/// </summary>
public class ApplicationRoleClaim : IdentityRoleClaim<int>, ICreationAudited, IUpdationAudited
{
    public ApplicationRoleClaim() : base() 
    { 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Gets or sets the ID of the user who created this role claim.
    /// </summary>
    [Required]
    public int CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this role claim was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who last updated this role claim.
    /// </summary>
    [Required]
    public int UpdatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this role claim was last updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }
}
