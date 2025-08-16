using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Identity;

/// <summary>
/// Represents a permission/claim in the system.
/// </summary>
public class Permission
{
    /// <summary>
    /// Gets or sets the unique identifier for the permission.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the permission (e.g., "user:read", "ticket:write").
    /// Maximum length is 20 characters.
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized name of the permission used for uniqueness checks.
    /// Maximum length is 30 characters. Has a unique index.
    /// </summary>
    [Required]
    [StringLength(30)]
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the user who created this permission.
    /// </summary>
    [Required]
    public Guid CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this permission was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the ID of the user who last updated this permission.
    /// </summary>
    [Required]
    public Guid UpdatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this permission was last updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
