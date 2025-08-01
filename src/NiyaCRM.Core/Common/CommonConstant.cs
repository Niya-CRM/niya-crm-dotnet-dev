namespace NiyaCRM.Core.Common;

public static class CommonConstant
{
    public static readonly Guid DEFAULT_USER = Guid.Parse("00000000-0000-0000-0000-000000000000");

    // Message Constants
    public const string MESSAGE_INVALID_REQUEST = "Invalid Request";
    public const string MESSAGE_CONFLICT = "Conflict";
    public const string MESSAGE_INTERNAL_SERVER_ERROR = "Internal Server Error";

    // Error Level
    public const string ERROR_LEVEL_INFORMATION = "Information";
    public const string ERROR_LEVEL_WARNING = "Warning";
    public const string ERROR_LEVEL_ERROR = "Error";
    public const string ERROR_LEVEL_FATAL = "Fatal";

    // Pagination Constants
    public const int PAGE_NUMBER_DEFAULT = 1;
    public const int PAGE_SIZE_DEFAULT = 50;
    public const int PAGE_SIZE_MIN = 1;
    public const int PAGE_SIZE_MAX = 100;

    public const string AUDIT_LOG_EVENT_CREATE = "Create";
    public const string AUDIT_LOG_EVENT_UPDATE = "Update";
    public const string AUDIT_LOG_EVENT_DELETE = "Delete";

    public const string AUDIT_LOG_MODULE_TENANT = "Tenant";

    /// <summary>
    /// Constants related to health checks
    /// </summary>
    public static class HealthCheck
    {
        /// <summary>
        /// Tags for service health checks
        /// </summary>
        public static readonly string[] ServiceTags = { "service" };
        
        /// <summary>
        /// Tags for database health checks
        /// </summary>
        public static readonly string[] DatabaseTags = { "database" };
    }
}
