using System.ComponentModel.DataAnnotations;

namespace OXDesk.Core.Tickets.DTOs;

/// <summary>
/// Request DTO for creating a new business hours schedule.
/// </summary>
public class CreateBusinessHoursRequest
{
    /// <summary>
    /// Gets or sets the name of the business hours schedule.
    /// </summary>
    [Required]
    [StringLength(60)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    [StringLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether this is the default business hours schedule.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets the time zone.
    /// </summary>
    [StringLength(100)]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Gets or sets the business hours type (e.g., "24x7" or "custom").
    /// </summary>
    [Required]
    [StringLength(30)]
    public string BusinessHoursType { get; set; } = BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven;

    /// <summary>
    /// Gets or sets the custom business hours entries. Should be null or empty when type is "24x7".
    /// </summary>
    public List<CustomBusinessHoursItem>? CustomHours { get; set; }
}

/// <summary>
/// Request DTO for patching (partial update) an existing business hours schedule.
/// </summary>
public class PatchBusinessHoursRequest
{
    /// <summary>
    /// Gets or sets the name of the business hours schedule.
    /// </summary>
    [StringLength(60)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    [StringLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether this is the default business hours schedule.
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the time zone.
    /// </summary>
    [StringLength(100)]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Gets or sets the business hours type (e.g., "24x7" or "custom").
    /// </summary>
    [StringLength(30)]
    public string? BusinessHoursType { get; set; }

    /// <summary>
    /// Gets or sets the custom business hours entries. When provided, replaces all existing custom hours.
    /// Ignored or should be null/empty when type is "24x7".
    /// </summary>
    public List<CustomBusinessHoursItem>? CustomHours { get; set; }
}

/// <summary>
/// A single custom business hours entry within a create or patch request.
/// </summary>
public class CustomBusinessHoursItem
{
    /// <summary>
    /// Gets or sets the day of the week (e.g., "Sunday", "Monday").
    /// </summary>
    [Required]
    [StringLength(10)]
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

/// <summary>
/// Request DTO for creating a new holiday entry.
/// </summary>
public class CreateHolidayRequest
{
    /// <summary>
    /// Gets or sets the parent business hours identifier.
    /// </summary>
    [Required]
    public int BusinessHourId { get; set; }

    /// <summary>
    /// Gets or sets the name of the holiday.
    /// </summary>
    [Required]
    [StringLength(60)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of the holiday.
    /// </summary>
    [Required]
    public DateOnly Date { get; set; }
}
