using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Represents a business hours schedule configuration.
/// </summary>
[Table("business_hours")]
public class BusinessHours : AuditedEntityWithSoftDelete, IEntity
{
    /// <inheritdoc />
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the business hours schedule.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description of the business hours schedule.
    /// </summary>
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether this is the default business hours schedule.
    /// </summary>
    [Required]
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets the time zone for the business hours schedule.
    /// </summary>
    [Required]
    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;

    /// <summary>
    /// Gets or sets the type of business hours (e.g., "24x7" or "custom").
    /// </summary>
    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string BusinessHoursType { get; set; } = BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven;
}
