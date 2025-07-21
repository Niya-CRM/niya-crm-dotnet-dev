namespace NiyaCRM.Core.Common;

public static class CommonConstant
{
    public const int PAGE_NUMBER_DEFAULT = 1;
    public const int PAGE_SIZE_DEFAULT = 50;
    public const int PAGE_SIZE_MIN = 1;
    public const int PAGE_SIZE_MAX = 100;

    public const string AUDIT_LOG_EVENT_CREATE = "Create";
    public const string AUDIT_LOG_EVENT_UPDATE = "Update";
    public const string AUDIT_LOG_EVENT_DELETE = "Delete";

    public const string AUDIT_LOG_MODULE_TENANT = "Tenant";
}
