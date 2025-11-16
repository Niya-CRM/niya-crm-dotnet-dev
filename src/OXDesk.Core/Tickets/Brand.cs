using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Represents a brand entity in the ticketing system.
/// </summary>
[Table("brands")]
public class Brand : AuditedEntityWithSoftDelete, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the brand name.
    /// </summary>
    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string BrandName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the logo URL.
    /// </summary>
    [StringLength(1000)]
    [Column(TypeName = "varchar(1000)")]
    public string? Logo { get; set; }

    /// <summary>
    /// Gets or sets the dark mode logo URL.
    /// </summary>
    [StringLength(1000)]
    [Column(TypeName = "varchar(1000)")]
    public string? LogoDark { get; set; }

    /// <summary>
    /// Gets or sets the brand color (hex code).
    /// </summary>
    [StringLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string? BrandColor { get; set; }

    /// <summary>
    /// Gets or sets the brand website URL.
    /// </summary>
    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? Website { get; set; }

    // Audit fields inherited from AuditedEntityWithSoftDelete:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
}
