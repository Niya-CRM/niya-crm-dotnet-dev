using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.AuditLogs.ChangeHistory;

/// <summary>
/// Represents a change history log entry in the CRM system.
/// Tracks field-level changes to entities for maintaining history.
/// Append-only entity - no updates or soft deletes.
/// </summary>
[Table("change_history_logs")]
public class ChangeHistoryLog : CreationAuditedEntity, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the change history log entry.
    /// </summary>
    [Key]
    [Column("id")]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the object ID.
    /// </summary>
    [Column("object_id")]
    public int ObjectId { get; set; }

    /// <summary>
    /// Gets or sets the integer ID of the object item (entity) that was changed.
    /// </summary>
    [Column("object_item_id_int")]
    public int? ObjectItemIdInt { get; set; }

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
    /// Gets or sets the correlation ID for tracking related operations.
    /// </summary>
    [Column("correlation_id")]
    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    // Audit fields inherited from CreationAuditedEntity:
    // - CreatedAt, CreatedBy (append-only, no updates)

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeHistoryLog"/> class.
    /// Private parameterless constructor for Entity Framework.
    /// </summary>
    private ChangeHistoryLog()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeHistoryLog"/> class.
    /// Constructor for creating new change history log entries with int-based entity ID.
    /// </summary>
    /// <param name="objectId">The object key (entity type).</param>
    /// <param name="entityId">The object item ID (entity ID) as int.</param>
    /// <param name="fieldName">The name of the field that was changed.</param>
    /// <param name="oldValue">The previous value of the field.</param>
    /// <param name="newValue">The new value of the field.</param>
    /// <param name="changedBy">The user who created the change.</param>
    public ChangeHistoryLog(
        int objectId,
        int entityId,
        string fieldName,
        string? oldValue,
        string? newValue,
        int changedBy)
    {
        ObjectId = objectId;
        ObjectItemIdInt = entityId;
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = changedBy;
    }
}

