using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NiyaCRM.Core.ValueLists;

/// <summary>
/// Represents a Value List entity in the CRM system.
/// </summary>
[Table("value_lists")]
public class ValueList
{
    /// <summary>
    /// Gets or sets the unique identifier for the value list.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the value list.
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the value list.
    /// </summary>
    [Required]
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value list type.
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(10)")]
    public string ValueListType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the value list is active.
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(1)")]
    public string IsActive { get; set; } = "Y";

    /// <summary>
    /// Gets or sets the date and time when the value list was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the value list was last updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the value list.
    /// </summary>
    [Required]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the value list.
    /// </summary>
    [Required]
    public Guid UpdatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public ValueList() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList"/> class.
    /// Constructor for creating new value list instances with all required properties.
    /// </summary>
    public ValueList(
        Guid id,
        string name,
        string description,
        string valueListTypeId,
        string isActive,
        DateTime createdAt,
        Guid createdBy)
    {
        Id = id;
        Name = name;
        Description = description;
        ValueListType = valueListTypeId;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }
}
