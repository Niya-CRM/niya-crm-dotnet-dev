using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.FormTypes;

/// <summary>
/// Represents a Form type lookup.
/// </summary>
[Table("form_types")]
public class FormType : AuditedEntityWithSoftDelete, IEntity
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
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? Description { get; set; }

    // Audit fields inherited from AuditedEntityWithSoftDelete:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
}
