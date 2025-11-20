using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.ValueLists;

/// <summary>
/// Represents a Value List entity in the CRM system.
/// </summary>
[Table("value_lists")]
public class ValueList : AuditedEntityWithSoftDelete, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the value list.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the value list.
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string ListName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the value list.
    /// </summary>
    [Required]
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value list type - Standard or Custom
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(10)")]
    public string ValueListType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the value list is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether allowed to modify.
    /// </summary>
    [Required]
    public bool AllowModify { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether allowed to add new item.
    /// </summary>
    [Required]
    public bool AllowNewItem { get; set; } = true;

    // Audit fields inherited from AuditedEntity:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public ValueList() { }

    /// <summary>
    /// Overload without Id for identity-based key generation.
    /// </summary>
    public ValueList(
        string listName,
        string description,
        string valueListTypeId,
        bool isActive,
        bool allowModify,
        bool allowNewItem,
        Guid createdBy)
    {
        ListName = listName;
        Description = description;
        ValueListType = valueListTypeId;
        IsActive = isActive;
        AllowModify = allowModify;
        AllowNewItem = allowNewItem;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }
}
