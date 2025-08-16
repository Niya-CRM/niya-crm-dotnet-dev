namespace OXDesk.Core.DynamicObjects;

/// <summary>
/// Constants related to Dynamic Objects in the CRM system.
/// </summary>
public static class DynamicObjectConstants
{
    /// <summary>
    /// Constants for Dynamic Object types.
    /// </summary>
    public static class ObjectTypes
    {
        /// <summary>
        /// Standard object type - used for system-defined objects that are common across all CRM implementations.
        /// </summary>
        public const string Standard = "standard";

        /// <summary>
        /// Dedicated object type - used for objects that are specifically designed for certain business domains.
        /// </summary>
        public const string Dedicated = "dedicated";

        /// <summary>
        /// Custom object type - used for user-defined objects that are specific to a tenant's implementation.
        /// </summary>
        public const string Custom = "custom";
    }

    public static class DynamicObjectNames
    {
        public const string User = "user";
        public const string Account = "account";
        public const string Contact = "contact";
        public const string Ticket = "ticket";
        
        public static readonly string[] All = 
        {
            User,
            Account,
            Contact,
            Ticket
        };
    }
}
