using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.ValueLists;

namespace OXDesk.Core.DynamicObjects.Fields;

/// <summary>
/// Represents an available field type definition that can be used by dynamic object fields.
/// </summary>
[Table("dynamic_object_field_types")]
public class DynamicObjectFieldType
{
    /// <summary>
    /// Unique identifier for the field type.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Human-friendly display name for the field type.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stable key for the field type (e.g., "text", "number", "date", "file").
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string FieldTypeKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of what this field type represents.
    /// </summary>
    [Required]
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string Description { get; set; } = string.Empty;

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
    /// Minimum number of items allowed in the list or checkbox or radio.
    /// </summary>
    public int? MinSelectedItems { get; set; }

    /// <summary>
    /// Maximum number of items allowed in the list or checkbox or radio.
    /// </summary>
    public int? MaxSelectedItems { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the field type was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the field type was deleted (soft delete).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the field type was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the field type.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the field type.
    /// </summary>
    public Guid UpdatedBy { get; set; }
}
