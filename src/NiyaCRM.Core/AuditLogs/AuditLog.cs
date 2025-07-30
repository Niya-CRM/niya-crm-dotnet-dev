using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NiyaCRM.Core.AuditLogs;

/// <summary>
/// Represents an audit log entry in the CRM system.
/// Pure domain entity for tracking system activities.
/// </summary>
[Table("audit_logs")]
public class AuditLog
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit log entry.
    /// </summary>
    [Key]
    [Column("id")]
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the module/entity type that was affected.
    /// </summary>
    [Column("module")]
    [Required]
    [MaxLength(100)]
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event/action that occurred.
    /// </summary>
    [Column("event")]
    [Required]
    [MaxLength(100)]
    public string Event { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the entity that was affected.
    /// </summary>
    [Column("mapped_id")]
    [Required]
    [MaxLength(100)]
    public string MappedId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IP address from which the action was performed.
    /// </summary>
    [Column("ip")]
    [MaxLength(45)]
    public string IP { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the any data generated for the action.
    /// </summary>
    [Column("data")]
    [MaxLength(1000)]
    public string? Data { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the audit log was created.
    /// </summary>
    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who performed the action.
    /// </summary>
    [Column("created_by")]
    [Required]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLog"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public AuditLog() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLog"/> class.
    /// Constructor for creating new audit log entries.
    /// </summary>
    /// <param name="id">The audit log identifier.</param>
    /// <param name="module">The module/entity type.</param>
    /// <param name="event">The event/action type.</param>
    /// <param name="mappedId">The affected entity ID.</param>
    /// <param name="ip">The IP address.</param>
    /// <param name="data">The audit data.</param>
    /// <param name="createdBy">The user who performed the action.</param>
    public AuditLog(Guid id, string module, string @event, string mappedId, string ip, string? data, Guid createdBy)
    {
        Id = id;
        Module = module;
        Event = @event;
        MappedId = mappedId;
        IP = ip;
        Data = data;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }
}
