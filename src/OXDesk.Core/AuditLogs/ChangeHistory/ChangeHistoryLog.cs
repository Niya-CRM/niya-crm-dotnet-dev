using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.AuditLogs.ChangeHistory;

/// <summary>
/// Represents a change history log entry in the CRM system.
/// Tracks field-level changes to entities for maintaining history.
/// </summary>
[Table("change_history_logs")]
public class ChangeHistoryLog
{
    /// <summary>
    /// Gets or sets the unique identifier for the change history log entry.
    /// </summary>
    [Key]
    [Column("id")]
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public int TenantId { get; set; }

    /// <summary>
    /// Gets or sets the object key (entity type) that was changed.
    /// </summary>
    [Column("object_key")]
    [Required]
    [MaxLength(100)]
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the object item (entity) that was changed.
    /// </summary>
    [Column("object_item_id")]
    [Required]
    public Guid ObjectItemId { get; set; }

    /// <summary>
    /// Gets or sets the name of the field that was changed.
    /// </summary>
    [Column("field_name")]
    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous value of the field before the change.
    /// </summary>
    [Column("old_value")]
    [MaxLength(1000)]
    public string? OldValue { get; set; }

    /// <summary>
    /// Gets or sets the new value of the field after the change.
    /// </summary>
    [Column("new_value")]
    [MaxLength(1000)]
    public string? NewValue { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the change was created.
    /// </summary>
    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the change.
    /// </summary>
    [Column("created_by")]
    [Required]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeHistoryLog"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public ChangeHistoryLog() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeHistoryLog"/> class.
    /// Constructor for creating new change history log entries.
    /// </summary>
    /// <param name="id">The change history log identifier.</param>
    /// <param name="entityType">The object key (entity type).</param>
    /// <param name="entityId">The object item ID (entity ID).</param>
    /// <param name="fieldName">The name of the field that was changed.</param>
    /// <param name="oldValue">The previous value of the field.</param>
    /// <param name="newValue">The new value of the field.</param>
    /// <param name="changedBy">The user who created the change.</param>
    public ChangeHistoryLog(
        Guid id,
        string entityType,
        Guid entityId,
        string fieldName,
        string? oldValue,
        string? newValue,
        Guid changedBy)
    {
        Id = id;
        ObjectKey = entityType;
        ObjectItemId = entityId;
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = changedBy;
    }
}

