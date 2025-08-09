using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NiyaCRM.Core.ValueLists;

namespace NiyaCRM.Core.DynamicObjects.Fields;

/// <summary>
/// Represents a field definition for a Dynamic Object.
/// </summary>
[Table("dynamic_object_fields")]
public class DynamicObjectField
{
    /// <summary>
    /// Primary key.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Object key referring to the owning DynamicObject (by key, not FK to Id).
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Unique key of the field within the object.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string FieldKey { get; set; } = string.Empty;

    /// <summary>
    /// Display label for the field.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Field data type (string representation).
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string FieldType { get; set; } = string.Empty;

    /// <summary>
    /// To be indexed or not.
    /// </summary>
    public bool Indexed { get; set; }

    /// <summary>
    /// Short description.
    /// </summary>
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? Description { get; set; }

    /// <summary>
    /// Help text shown to users.
    /// </summary>
    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? HelpText { get; set; }

    /// <summary>
    /// Placeholder text for input fields.
    /// </summary>
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Whether this field is required.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Whether values must be unique across records.
    /// </summary>
    public bool Unique { get; set; }

    /// <summary>
    /// Minimum length constraint for text fields.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// Maximum length constraint for text fields.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Number of decimal places for numeric fields.
    /// </summary>
    public int? Decimals { get; set; }

    /// <summary>
    /// Maximum file size allowed (in MB) for file fields.
    /// </summary>
    public int? MaxFileSize { get; set; }

    /// <summary>
    /// Allowed file extensions (comma-separated) for file fields.
    /// </summary>
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? AllowedFileTypes { get; set; }

    /// <summary>
    /// Minimum number of files required for file fields.
    /// </summary>
    public int? MinFileCount { get; set; }

    /// <summary>
    /// Maximum number of files allowed for file fields.
    /// </summary>
    public int? MaxFileCount { get; set; }

    /// <summary>
    /// Reference to an optional value list backing this field.
    /// </summary>
    public Guid? ValueListId { get; set; }

    /// <summary>
    /// Navigation to the value list.
    /// </summary>
    public ValueList? ValueList { get; set; }

    /// <summary>
    /// Minimum number of items allowed in the list or checkbox or radio.
    /// </summary>
    public int? MinSelectedItems { get; set; }

    /// <summary>
    /// Maximum number of items allowed in the list or checkbox or radio.
    /// </summary>
    public int? MaxSelectedItems { get; set; }

    /// <summary>
    /// Whether the field is editable after initial submission.
    /// </summary>
    public bool EditableAfterSubmission { get; set; }

    /// <summary>
    /// Visibility flags for UI contexts.
    /// </summary>
    public bool VisibleOnCreate { get; set; }
    public bool VisibleOnEdit { get; set; }
    public bool VisibleOnView { get; set; }

    /// <summary>
    /// Whether changes to this field should be audited.
    /// </summary>
    public bool AuditChanges { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the dynamic field was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the dynamic field was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the dynamic field was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    /// <summary>
    /// Gets or sets the user who created the dynamic field.
    /// </summary>
    public Guid CreatedBy { get; set; }
    /// <summary>
    /// Gets or sets the user who last updated the dynamic field.
    /// </summary>
    public Guid UpdatedBy { get; set; }
}
