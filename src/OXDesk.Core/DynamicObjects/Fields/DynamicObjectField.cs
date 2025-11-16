using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.ValueLists;
using OXDesk.Core.Entities;

namespace OXDesk.Core.DynamicObjects.Fields;

/// <summary>
/// Represents a field definition for a Dynamic Object.
/// </summary>
[Table("dynamic_object_fields")]
public class DynamicObjectField : AuditedEntityWithSoftDelete, IEntity
{
    /// <summary>
    /// Primary key.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Object identifier referring to the owning DynamicObject (FK to Id).
    /// </summary>
    [Required]
    public int ObjectId { get; set; }    

    /// <summary>
    /// Field type identifier referencing DynamicObjectFieldType.
    /// </summary>
    [Required]
    public int FieldTypeId { get; set; }

    /// <summary>
    /// Display label for the field.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string Label { get; set; } = string.Empty;

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
    public int? ValueListId { get; set; }

    /// <summary>
    /// Minimum number of items allowed in the list or checkbox or radio.
    /// </summary>
    public int? MinSelectedItems { get; set; }

    /// <summary>
    /// Maximum number of items allowed in the list or checkbox or radio.
    /// </summary>
    public int? MaxSelectedItems { get; set; }

    /// <summary>
    /// Whether the field is editable by the user.
    /// </summary>
    public bool Editable { get; set; }

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

    // Audit fields inherited from AuditedEntityWithSoftDelete:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
}
