using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Tickets;

[Table("brands")]
public class Brand
{
    [Key]
    [Required]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public int TenantId { get; set; }

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string BrandName { get; set; } = string.Empty;

    [StringLength(1000)]
    [Column(TypeName = "varchar(1000)")]
    public string? Logo { get; set; }

    [StringLength(1000)]
    [Column(TypeName = "varchar(1000)")]
    public string? LogoDark { get; set; }

    [StringLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string? BrandColor { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? Website { get; set; }

    [Required]
    public int CreatedBy { get; set; }

    [Required]
    public int UpdatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
