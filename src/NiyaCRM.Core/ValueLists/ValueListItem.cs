using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NiyaCRM.Core.ValueLists;

/// <summary>
/// Represents a Value List Item entity in the CRM system.
/// </summary>
[Table("value_list_items")]
public class ValueListItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the value list item.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the value list item.
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key of the value list item.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string ItemValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value list ID this item belongs to.
    /// </summary>
    [Required]
    public Guid ValueListId { get; set; }

    /// <summary>
    /// Gets or sets the value list this item belongs to.
    /// </summary>
    [ForeignKey(nameof(ValueListId))]
    public ValueList? ValueList { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the value list item is active.
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(1)")]
    public string IsActive { get; set; } = "Y";

    /// <summary>
    /// Gets or sets the date and time when the value list item was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the value list item was last updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the value list item.
    /// </summary>
    [Required]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the value list item.
    /// </summary>
    [Required]
    public Guid UpdatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueListItem"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public ValueListItem() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueListItem"/> class.
    /// Constructor for creating new value list item instances with all required properties.
    /// </summary>
    public ValueListItem(
        Guid id,
        string itemName,
        string itemValue,
        Guid valueListId,
        string isActive,
        Guid createdBy)
    {
        Id = id;
        ItemName = itemName;
        ItemValue = itemValue;
        ValueListId = valueListId;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }
}
