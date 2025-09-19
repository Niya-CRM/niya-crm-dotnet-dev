using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.DynamicObjects;

/// <summary>
/// Represents a Dynamic Object entity in the CRM system.
/// </summary>
[Table("dynamic_objects")]
public class DynamicObject
{
    /// <summary>
    /// Gets or sets the unique identifier for the dynamic object.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public int TenantId { get; set; }

    /// <summary>
    /// Gets or sets the name of the dynamic object.
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string ObjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the singular name of the dynamic object.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string SingularName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plural name of the dynamic object.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string PluralName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key of the dynamic object.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the dynamic object.
    /// </summary>
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object type.
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(10)")]
    public string ObjectType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the dynamic object was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the dynamic object was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the dynamic object was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the dynamic object.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the dynamic object.
    /// </summary>
    public Guid UpdatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public DynamicObject() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObject"/> class.
    /// Constructor for creating new dynamic object instances with all required properties.
    /// </summary>
    public DynamicObject(
        string objectName,
        string singularName,
        string pluralName,
        string objectKey,
        string description,
        string objectType,
        Guid createdBy)
    {
        ObjectName = objectName;
        SingularName = singularName;
        PluralName = pluralName;
        ObjectKey = objectKey;
        Description = description;
        ObjectType = objectType;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }
}
