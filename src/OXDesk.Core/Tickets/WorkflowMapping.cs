using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Maps a workflow to a Topic/SubTopic for routing and processing.
/// </summary>
[Table("workflow_mappings")]
public class WorkflowMapping : AuditedEntityWithSoftDelete, IEntity, ITenantScoped
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
    /// Related workflow identifier.
    /// </summary>
    [Required]
    public int WorkFlowId { get; set; }

    /// <summary>
    /// Topic identifier.
    /// </summary>
    public int? TopicId { get; set; }

    /// <summary>
    /// SubTopic identifier.
    /// </summary>
    public int? SubTopicId { get; set; }

    // Audit fields inherited from AuditedEntityWithSoftDelete:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
}
