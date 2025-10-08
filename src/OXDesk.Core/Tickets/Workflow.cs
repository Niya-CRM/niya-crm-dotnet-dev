using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Represents a workflow step or definition used for ticket status transitions.
/// </summary>
[Table("workflows")]
public class Workflow : AuditedEntityWithSoftDelete, IEntity, ITenantScoped
{
    /// <summary>
    /// Primary key.
    /// </summary>
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public Guid TenantId { get; set; }

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
