using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.ValueLists;

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
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the value list.
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string ListName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique key of the value list (e.g., "countries").
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string ListKey { get; set; } = string.Empty;

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
    public int CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the value list.
    /// </summary>
    [Required]
    public int UpdatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public ValueList() { }

    /// <summary>
    /// Overload without Id for identity-based key generation.
    /// </summary>
    public ValueList(
        Guid tenantId,
        string listName,
        string listKey,
        string description,
        string valueListTypeId,
        bool isActive,
        bool allowModify,
        bool allowNewItem,
        int createdBy)
    {
        TenantId = tenantId;
        ListName = listName;
        ListKey = listKey;
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
