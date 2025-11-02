using System;

namespace OXDesk.Core.Common;

public static class CommonConstant
{

    // Technical Username
    public const string TECHNICAL_USERNAME = "OXDesk@system.local";

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
    public const string AUDIT_LOG_EVENT_LOGIN = "Login";

    public const string MODULE_TENANT = "tenants";
    public const string MODULE_USER = "users";
    public const string MODULE_TICKET = "tickets";
    public const string MODULE_CONTACT = "contacts";
    public const string MODULE_ACCOUNT = "accounts";
    public const string MODULE_ROLE = "roles";
    public const string MODULE_PERMISSION = "permissions";
    public const string MODULE_TEAM = "teams";
    public const string MODULE_DEPARTMENT = "departments";
    public const string MODULE_ORGANISATION = "organisations";
    public const string MODULE_BRAND = "brands";
    public const string MODULE_PRODUCT = "products";
    public const string MODULE_TEMPLATE = "templates";
    public const string MODULE_MESSAGE = "messages";

    // Change History field constants
    public static class ChangeHistoryFields
    {
        public const string Created = "created";
        public const string Deleted = "deleted";
    }

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
        public const string AccountOwner = "Account Owner";
        public const string Administrator = "Administrator";
        public const string PowerUser = "Power User";
        public const string SupportAgent = "Support Agent";
        public const string LightAgent = "Light Agent";
        public const string EndUser = "End User";
        public const string System = "System";
        
        public static readonly string[] All = 
        {
            AccountOwner,
            Administrator,
            PowerUser,
            SupportAgent,
            LightAgent,
            EndUser,
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
        /// Cache key for the roles list.
        /// </summary>
        public const string RolesList = "role:list";

        /// <summary>
        /// Cache key for the permissions list.
        /// </summary>
        public const string PermissionsList = "permission:list";

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
        public const string Languages = "languages";
        public const string RequestTypes = "request-types";
        public const string UserProfiles = "user-profiles";
    }
    
    /// <summary>
    /// User Profile constants with display Name and kebab-case Key.
    /// </summary>
    public static class UserProfiles
    {
        public static class Agent { public const string Name = "Agent"; public const string Key = "agent"; }
        public static class AIAgent { public const string Name = "AI Agent"; public const string Key = "ai-agent"; }
        public static class LightAgent { public const string Name = "Light Agent"; public const string Key = "light-agent"; }
        public static class EndUser { public const string Name = "End User"; public const string Key = "end-user"; }
        public static class System { public const string Name = "System"; public const string Key = "system"; }

        public static readonly (string Name, string Key)[] All =
        [
            (Agent.Name, Agent.Key),
            (AIAgent.Name, AIAgent.Key),
            (LightAgent.Name, LightAgent.Key),
            (EndUser.Name, EndUser.Key),
            (System.Name, System.Key)
        ];
    }

    /// <summary>
    /// Constants related to health checks
    /// </summary>
    public static class HealthCheck
    {
        /// <summary>
        /// Tags for service health checks
        /// </summary>
        public static readonly string[] ServiceTags = ["service"];
        
        /// <summary>
        /// Tags for database health checks
        /// </summary>
        public static readonly string[] DatabaseTags = ["database"];
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
            [
                Initial,
                Upgrade
            ];
        }

        public const string INITIAL_VERSION = "0.0.1";
    }
}
