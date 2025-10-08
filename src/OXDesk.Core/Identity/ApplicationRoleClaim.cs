using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Identity;

/// <summary>
/// Custom application role claim that uses Guid as primary key with audit fields.
/// </summary>
public class ApplicationRoleClaim : IdentityRoleClaim<Guid>, ICreationAudited, IUpdationAudited, ITenantScoped
{
    public ApplicationRoleClaim() : base() 
    { 
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.Column("tenant_id")]
    [Required]
    public Guid TenantId { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who created this role claim.
    /// </summary>
    [Required]
    public Guid CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this role claim was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who last updated this role claim.
    /// </summary>
    [Required]
    public Guid UpdatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this role claim was last updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }
}
