using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Maps a workflow to a Topic/SubTopic for routing and processing.
/// </summary>
[Table("workflow_mappings")]
public class WorkflowMapping
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

    /// <summary>
    /// Audit fields.
    /// </summary>
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
