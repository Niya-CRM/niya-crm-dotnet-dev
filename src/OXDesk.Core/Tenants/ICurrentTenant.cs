using System;

namespace OXDesk.Core.Tenants
{
    /// <summary>
    /// Represents the ambient current tenant within the scoped request.
    /// </summary>
    public interface ICurrentTenant
    {
        /// <summary>
        /// Current tenant id for the active scope/request. Null means not set.
        /// </summary>
        Guid? Id { get; }

        /// <summary>
        /// Change the current tenant id for this scope.
        /// </summary>
        /// <param name="tenantId">The tenant id to set. Null clears.</param>
        void Change(Guid? tenantId);

        /// <summary>
        /// Temporarily change the current tenant id and restore the previous value when disposed.
        /// </summary>
        /// <param name="tenantId">The tenant id to set. Null clears.</param>
        /// <returns>An IDisposable that restores the previous tenant id upon dispose.</returns>
        IDisposable ChangeScoped(Guid? tenantId);
    }
}
