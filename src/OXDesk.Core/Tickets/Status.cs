using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Tickets;

[Table("statuses")]
public class Status : AuditedEntityWithSoftDelete, IEntity
{
    [Key]
    [Required]
    public int Id { get; set; }

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

    // Audit fields inherited from AuditedEntityWithSoftDelete:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
}
