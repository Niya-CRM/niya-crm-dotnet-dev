using System;
using System.Linq;

namespace OXDesk.Core.Identity
{
    /// <summary>
    /// Convenience extension methods for ICurrentUser.
    /// </summary>
    public static class CurrentUserExtensions
    {
        /// <summary>
        /// Returns true if the current user has the specified role (case-insensitive).
        /// </summary>
        public static bool IsInRole(this ICurrentUser currentUser, string role)
        {
            if (currentUser == null) return false;
            if (string.IsNullOrWhiteSpace(role)) return false;
            return currentUser.Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns true if the current user has the specified permission (case-insensitive).
        /// </summary>
        public static bool HasPermission(this ICurrentUser currentUser, string permission)
        {
            if (currentUser == null) return false;
            if (string.IsNullOrWhiteSpace(permission)) return false;
            return currentUser.Permissions.Any(p => string.Equals(p, permission, StringComparison.OrdinalIgnoreCase));
        }
    }
}
