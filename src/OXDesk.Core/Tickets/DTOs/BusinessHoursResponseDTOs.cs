using OXDesk.Core.Common.DTOs;

namespace OXDesk.Core.Tickets.DTOs;

/// <summary>
/// Response DTO for a business hours schedule.
/// </summary>
public class BusinessHoursResponse : AuditedDto
{
    /// <summary>
    /// Gets or sets the name of the business hours schedule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether this is the default business hours schedule.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the time zone.
    /// </summary>
    public string TimeZone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the business hours type.
    /// </summary>
    public string BusinessHoursType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the soft deletion timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the display name of the user who created the record.
    /// </summary>
    public string? CreatedByText { get; set; }

    /// <summary>
    /// Gets or sets the display name of the user who last updated the record.
    /// </summary>
    public string? UpdatedByText { get; set; }
}

/// <summary>
/// Response DTO for custom business hours.
/// </summary>
public class CustomBusinessHoursResponse : AuditedDto
{
    /// <summary>
    /// Gets or sets the parent business hours identifier.
    /// </summary>
    public int BusinessHourId { get; set; }

    /// <summary>
    /// Gets or sets the day of the week.
    /// </summary>
    public string Day { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time.
    /// </summary>
    public TimeOnly EndTime { get; set; }
}

/// <summary>
/// Response DTO for a holiday.
/// </summary>
public class HolidayResponse : AuditedDto
{
    /// <summary>
    /// Gets or sets the parent business hours identifier.
    /// </summary>
    public int BusinessHourId { get; set; }

    /// <summary>
    /// Gets or sets the name of the holiday.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of the holiday.
    /// </summary>
    public DateOnly Date { get; set; }
}

/// <summary>
/// Related data for a business hours details response.
/// </summary>
public class BusinessHoursDetailsRelated
{
    /// <summary>
    /// Gets or sets the custom business hours entries.
    /// </summary>
    public List<CustomBusinessHoursResponse> CustomBusinessHours { get; set; } = [];

    /// <summary>
    /// Gets or sets the holiday entries.
    /// </summary>
    public List<HolidayResponse> Holidays { get; set; } = [];
}
