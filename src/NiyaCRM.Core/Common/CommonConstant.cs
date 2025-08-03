namespace NiyaCRM.Core.Common;

public static class CommonConstant
{
    public static readonly Guid DEFAULT_TECHNICAL_USER = Guid.Parse("01986f5e-bce5-7835-be32-7ddc3ecbff4f");

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

    public static class PermissionNames
    {
        public const string SysSetupRead = "syssetup:read";
        public const string SysSetupWrite = "syssetup:write";
        public const string UserRead = "user:read";
        public const string UserWrite = "user:write";
        public const string TicketRead = "ticket:read";
        public const string TicketWrite = "ticket:write";
        public const string ContactRead = "contact:read";
        public const string ContactWrite = "contact:write";
        public const string AccountRead = "account:read";
        public const string AccountWrite = "account:write";
        public const string TemplateRead = "template:read";
        public const string TemplateWrite = "template:write";

        public static readonly string[] All =
        {
            SysSetupRead,
            SysSetupWrite,
            UserRead,
            UserWrite,
            TicketRead,
            TicketWrite,
            ContactRead,
            ContactWrite,
            AccountRead,
            AccountWrite,
            TemplateRead,
            TemplateWrite
        };
    }

    public static class RoleNames
    {
        public const string Administrator = "Administrator";
        public const string PowerUser = "Power User";
        public const string SupportAgent = "Support Agent";
        public const string LightAgent = "Light Agent";
        public const string ExternalUser = "External User";
        public const string Technical = "Technical";
        
        public static readonly string[] All = 
        {
            Administrator,
            PowerUser,
            SupportAgent,
            LightAgent,
            ExternalUser,
            Technical
        };
    }

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
