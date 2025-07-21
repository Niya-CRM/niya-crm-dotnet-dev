namespace NiyaCRM.Core.AuditLogs;

/// <summary>
/// Contains constants for audit logging functionality.
/// </summary>
public static class AuditLogConstant
{
    // Log Types for different modules
    public const string LOG_TYPE_TENANT = "TENANT";
    public const string LOG_TYPE_USER = "USER";
    public const string LOG_TYPE_CONTACT = "CONTACT";
    public const string LOG_TYPE_LEAD = "LEAD";
    public const string LOG_TYPE_OPPORTUNITY = "OPPORTUNITY";
    
    // Event Types
    public const string EVENT_CREATE = "CREATE";
    public const string EVENT_UPDATE = "UPDATE";
    public const string EVENT_DELETE = "DELETE";
    public const string EVENT_VIEW = "VIEW";
    public const string EVENT_LOGIN = "LOGIN";
    public const string EVENT_LOGOUT = "LOGOUT";
}
