using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Represents custom business hours for a specific day of the week.
/// </summary>
[Table("custom_business_hours")]
public class CustomBusinessHours : AuditedEntityWithSoftDelete, IEntity
{
    /// <inheritdoc />
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the associated business hours schedule identifier.
    /// </summary>
    [Required]
    public int BusinessHourId { get; set; }

    /// <summary>
    /// Gets or sets the day of the week (e.g., "Sunday", "Monday").
    /// </summary>
    [Required]
    [StringLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string Day { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time in 24-hour format.
    /// </summary>
    [Required]
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time in 24-hour format.
    /// </summary>
    [Required]
    public TimeOnly EndTime { get; set; }
}
