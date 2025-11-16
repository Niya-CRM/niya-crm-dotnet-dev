using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.ValueLists;

/// <summary>
/// Represents a Value List Item entity in the CRM system.
/// </summary>
[Table("value_list_items")]
public class ValueListItem : AuditedEntity, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the value list item.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the list Id this item belongs to.
    /// </summary>
    [Required]
    [Column(TypeName = "int")]
    public int ListId { get; set; }

    /// <summary>
    /// Gets or sets the value list this item belongs to.
    /// </summary>
    [ForeignKey(nameof(ListId))]
    public ValueList? ValueList { get; set; }

    /// <summary>
    /// Gets or sets the name of the value list item.
    /// </summary>
    [Required]
    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key of the value list item.
    /// </summary>
    [Required]
    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string ItemKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the value list item is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional custom order for sorting.
    /// Lower values come first; null means no custom order and sorts after ordered items.
    /// </summary>
    public int? Order { get; set; }

    // Audit fields inherited from AuditedEntity:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueListItem"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public ValueListItem() { }

    /// <summary>
    /// Overload without Id for identity-based key generation.
    /// </summary>
    public ValueListItem(
        string itemName,
        string itemKey,
        int listId,
        bool isActive,
        Guid createdBy,
        int? order = null)
    {
        ItemName = itemName;
        ItemKey = itemKey;
        ListId = listId;
        Order = order;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }
}


