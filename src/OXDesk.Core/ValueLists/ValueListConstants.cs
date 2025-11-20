namespace OXDesk.Core.ValueLists;

/// <summary>
/// Constants related to Value Lists in the CRM system.
/// </summary>
public static class ValueListConstants
{
    /// <summary>
    /// Constants for Value List types.
    /// </summary>
    public static class ValueListTypes
    {
        /// <summary>
        /// Standard value list type - used for system-defined value lists that are common across all implementations.
        /// </summary>
        public const string Standard = "Standard";

        /// <summary>
        /// Custom value list type - used for user-defined value lists that are specific to a tenant's implementation.
        /// </summary>
        public const string Custom = "Custom";

        /// <summary>
        /// System value list type - used for system-defined value lists that are common across all implementations and not displayed to user.
        /// </summary>
        public const string System = "System";
    }
}
