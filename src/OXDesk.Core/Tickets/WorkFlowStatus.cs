using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Represents a status within a workflow.
/// </summary>
[Table("workflow_statuses")]
public class WorkFlowStatus
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
    public int TenantId { get; set; }

    /// <summary>
    /// Related workflow identifier.
    /// </summary>
    [Required]
    public int WorkFlowId { get; set; }

    /// <summary>
    /// Related status identifier in the workflow.
    /// </summary>
    [Required]
    public int StatusId { get; set; }

    /// <summary>
    /// Name of the workflow status.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string WorkFlowStatusName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this is the default status in the workflow.
    /// </summary>
    [Required]
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Display order of the status within the workflow.
    /// </summary>
    [Required]
    public int Order { get; set; }

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
