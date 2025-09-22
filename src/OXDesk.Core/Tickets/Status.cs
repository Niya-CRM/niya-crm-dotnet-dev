using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Tickets;

[Table("statuses")]
public class Status
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

    /// <summary>
    /// Object identifier referring to the owning DynamicObject.
    /// </summary>
    [Required]
    public int ObjectId { get; set; } 

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string StatusName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string StatusType { get; set; } = string.Empty;

    [Required]
    public bool IsDefault { get; set; } = false;

    [Required]
    public int Order { get; set; }

    [Required]
    public int CreatedBy { get; set; }

    [Required]
    public int UpdatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
