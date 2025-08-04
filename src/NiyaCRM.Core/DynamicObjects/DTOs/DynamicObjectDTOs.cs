using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Core.DynamicObjects.DTOs;

/// <summary>
/// Request DTO for creating or updating a dynamic object.
/// </summary>
public class DynamicObjectRequest
{
    /// <summary>
    /// Gets or sets the name of the dynamic object.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ObjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the singular name of the dynamic object.
    /// </summary>
    [Required]
    [StringLength(60)]
    public string SingularName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plural name of the dynamic object.
    /// </summary>
    [Required]
    [StringLength(60)]
    public string PluralName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the dynamic object.
    /// </summary>
    [StringLength(255)]
    public string Description { get; set; } = string.Empty;
}
