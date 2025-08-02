namespace NiyaCRM.Core.Tenants
{
    public static class TenantConstant
    {
        public const string MESSAGE_TENANT_NOT_FOUND = "Tenant not found";
        
        /// <summary>
        /// Constants for tenant activation actions
        /// </summary>
        public static class ActivationAction
        {
            /// <summary>
            /// Action to activate a tenant
            /// </summary>
            public const string Activate = "activate";
            
            /// <summary>
            /// Action to deactivate a tenant
            /// </summary>
            public const string Deactivate = "deactivate";
        }
    }
}