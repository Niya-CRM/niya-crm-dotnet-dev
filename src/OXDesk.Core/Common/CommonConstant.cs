namespace OXDesk.Core.Common;

public static class CommonConstant
{
    public static readonly Guid DEFAULT_SYSTEM_USER = Guid.Parse("01986f5e-bce5-7835-be32-7ddc3ecbff4f");

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

    public const string AUDIT_LOG_MODULE_TENANT = "tenant";
    public const string AUDIT_LOG_MODULE_USER = "user";
    public const string AUDIT_LOG_MODULE_TICKET = "ticket";
    public const string AUDIT_LOG_MODULE_CONTACT = "contact";
    public const string AUDIT_LOG_MODULE_ACCOUNT = "account";

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
        public const string System = "System";
        
        public static readonly string[] All = 
        {
            Administrator,
            PowerUser,
            SupportAgent,
            LightAgent,
            ExternalUser,
            System
        };
    }

    /// <summary>
    /// Centralized cache keys used across the application.
    /// </summary>
    public static class CacheKeys
    {
        /// <summary>
        /// Cache key for the default paged user list (pageNumber=PAGE_NUMBER_DEFAULT, pageSize=PAGE_SIZE_DEFAULT).
        /// </summary>
        public const string UserList = "user:list";

        /// <summary>
        /// Cache keys for commonly used value lists.
        /// </summary>
        public static class ValueLists
        {
            public const string Countries = "valuelist:items:countries";
            public const string Currencies = "valuelist:items:currencies";
            public const string UserProfiles = "valuelist:items:user-profiles";
        }
    }

    /// <summary>
    /// Centralized names for commonly used value lists.
    /// </summary>
    public static class ValueListKeys
    {
        public const string Countries = "countries";
        public const string Currencies = "currencies";
        public const string UserProfiles = "user-profiles";
    }
    
    /// <summary>
    /// User Profile constants with display Name and kebab-case Key.
    /// </summary>
    public static class UserProfiles
    {
        public static class Agent { public const string Name = "Agent"; public const string Key = "agent"; }
        public static class LightAgent { public const string Name = "Light Agent"; public const string Key = "light-agent"; }
        public static class ExternalUser { public const string Name = "External User"; public const string Key = "external-user"; }
        public static class System { public const string Name = "System"; public const string Key = "system"; }

        public static readonly (string Name, string Key)[] All =
        {
            (Agent.Name, Agent.Key),
            (LightAgent.Name, LightAgent.Key),
            (ExternalUser.Name, ExternalUser.Key),
            (System.Name, System.Key)
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
    
    /// <summary>
    /// Constants related to application installation
    /// </summary>
    public static class AppInstallation
    {
        /// <summary>
        /// Pipeline types for application installation
        /// </summary>
        public static class Pipeline
        {
            public const string Initial = "Initial";
            public const string Upgrade = "Upgrade";
            
            public static readonly string[] All = 
            {
                Initial,
                Upgrade
            };
        }

        public const string INITIAL_VERSION = "0.0.1";
    }
}
