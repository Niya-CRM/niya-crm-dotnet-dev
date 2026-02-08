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
        /// System object type - used for system-defined objects that cannot be viewed or modified by the user.
        /// </summary>
        public const string System = "system";

        /// <summary>
        /// Custom object type - used for user-defined objects that are specific to a tenant's implementation.
        /// </summary>
        public const string Custom = "custom";
    }

    public static class DynamicObjectNames
    {
        public const string User = "Users";
        public const string Role = "Roles";
        public const string Permission = "Permissions";
        public const string Account = "Accounts";
        public const string Contact = "Contacts";
        public const string Ticket = "Tickets";
        public const string Tenant = "Tenants";
        public const string Brand = "Brands";
        public const string Organisation = "Organisations";
        public const string Department = "Departments";
        public const string Team = "Teams";
        public const string Product = "Products";
        
        public static readonly string[] All = 
        {
            User,
            Role,
            Permission,
            Account,
            Contact,
            Ticket,
            Tenant,
            Brand,
            Organisation,
            Department,
            Team,
            Product
        };
    }

    public static class DynamicObjectKeys
    {
        public const string User = "users";
        public const string Role = "roles";
        public const string Permission = "permissions";
        public const string Account = "accounts";
        public const string Contact = "contacts";
        public const string Ticket = "tickets";
        public const string Tenant = "tenants";
        public const string Brand = "brands";
        public const string Organisation = "organisations";
        public const string Department = "departments";
        public const string Team = "teams";
        public const string Product = "products";
        
        public static readonly string[] All = 
        {
            User,
            Role,
            Permission,
            Account,
            Contact,
            Ticket,
            Tenant,
            Brand,
            Organisation,
            Department,
            Team,
            Product
        };
    }
}
