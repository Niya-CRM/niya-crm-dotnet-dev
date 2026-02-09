namespace OXDesk.Core.Tickets;

/// <summary>
/// Constants related to business hours configuration.
/// </summary>
public static class BusinessHoursConstant
{
    /// <summary>
    /// Business hours type constants.
    /// </summary>
    public static class BusinessHoursTypes
    {
        /// <summary>
        /// 24 hours a day, 7 days a week.
        /// </summary>
        public const string TwentyFourSeven = "24x7";

        /// <summary>
        /// Custom business hours with specific days and times.
        /// </summary>
        public const string Custom = "custom";
    }

    /// <summary>
    /// Day of week name constants.
    /// </summary>
    public static class DayOfWeekNames
    {
        public const string Sunday = "Sunday";
        public const string Monday = "Monday";
        public const string Tuesday = "Tuesday";
        public const string Wednesday = "Wednesday";
        public const string Thursday = "Thursday";
        public const string Friday = "Friday";
        public const string Saturday = "Saturday";

        public static readonly string[] All =
        {
            Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday
        };
    }

    /// <summary>
    /// Error messages for business hours operations.
    /// </summary>
    public static class ErrorMessages
    {
        public const string BusinessHoursNotFound = "Business hours not found.";
        public const string CustomBusinessHoursNotFound = "Custom business hours not found.";
        public const string HolidayNotFound = "Holiday not found.";
        public const string InvalidBusinessHoursType = "Invalid business hours type. Must be '24x7' or 'custom'.";
        public const string InvalidDay = "Invalid day of the week.";
        public const string TimeOverlap = "Time ranges overlap on the same day.";
    }
}
