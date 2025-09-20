using System;
using System.Collections.Generic;

namespace OXDesk.Core.Identity
{
    /// <summary>
    /// Represents the ambient current user within the scoped request.
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// Current user id for the active scope/request. Null means not set.
        /// </summary>
        Guid? Id { get; }

        /// <summary>
        /// Display name of the current user, when available.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Email of the current user, when available.
        /// </summary>
        string? Email { get; }

        /// <summary>
        /// Roles assigned to the current user in this scope.
        /// </summary>
        IReadOnlyCollection<string> Roles { get; }

        /// <summary>
        /// Permissions assigned to the current user in this scope.
        /// </summary>
        IReadOnlyCollection<string> Permissions { get; }

        /// <summary>
        /// Change the current user context for this scope.
        /// </summary>
        /// <param name="id">The user id to set. Null clears.</param>
        /// <param name="roles">Optional set of roles for the user.</param>
        /// <param name="permissions">Optional set of permissions for the user.</param>
        /// <param name="name">Optional display name for the user.</param>
        /// <param name="email">Optional email for the user.</param>
        void Change(Guid? id, IEnumerable<string>? roles = null, IEnumerable<string>? permissions = null, string? name = null, string? email = null);

        /// <summary>
        /// Temporarily change the current user context and restore the previous values when disposed.
        /// </summary>
        /// <param name="id">The user id to set. Null clears.</param>
        /// <param name="roles">Optional set of roles for the user.</param>
        /// <param name="permissions">Optional set of permissions for the user.</param>
        /// <param name="name">Optional display name for the user.</param>
        /// <param name="email">Optional email for the user.</param>
        /// <returns>An IDisposable that restores the previous user context upon dispose.</returns>
        IDisposable ChangeScoped(Guid? id, IEnumerable<string>? roles = null, IEnumerable<string>? permissions = null, string? name = null, string? email = null);
    }
}
