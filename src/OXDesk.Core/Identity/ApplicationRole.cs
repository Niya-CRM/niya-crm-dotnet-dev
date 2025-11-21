using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Identity;

/// <summary>
/// Custom application role that uses int as primary key with audit fields.
/// </summary>
public class ApplicationRole : IdentityRole<int>, ICreationAudited, IUpdationAudited
{
    public ApplicationRole() : base() 
    { 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public ApplicationRole(string roleName) : base(roleName) 
    { 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Gets or sets the ID of the user who created this role.
    /// </summary>
    [Required]
    public int CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this role was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who last updated this role.
    /// </summary>
    [Required]
    public int UpdatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this role was last updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }
}
