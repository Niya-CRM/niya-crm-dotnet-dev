using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Workflows;

/// <summary>
/// Represents a workflow step or definition used for ticket status transitions.
/// </summary>
[Table("workflows")]
public class Workflow : AuditedEntityWithSoftDelete, IEntity
{
    /// <summary>
    /// Primary key.
    /// </summary>
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Object identifier referring to the owning DynamicObject.
    /// </summary>
    [Required]
    public int ObjectId { get; set; }

    /// <summary>
    /// Workflow name.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string WorkFlowName { get; set; } = string.Empty;

    // Audit fields inherited from AuditedEntityWithSoftDelete:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
}
