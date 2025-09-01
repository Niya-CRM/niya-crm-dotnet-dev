using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.ValueLists;

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
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public Guid TenantId { get; set; }

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
    /// Gets or sets the list key this item belongs to.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string ListKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value list this item belongs to.
    /// </summary>
    [ForeignKey(nameof(ListKey))]
    public ValueList? ValueList { get; set; }

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
        Guid tenantId,
        string itemName,
        string itemKey,
        string listKey,
        bool isActive,
        Guid createdBy,
        int? order = null)
    {
        Id = id;
        TenantId = tenantId;
        ItemName = itemName;
        ItemKey = itemKey;
        ListKey = listKey;
        Order = order;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }
}


