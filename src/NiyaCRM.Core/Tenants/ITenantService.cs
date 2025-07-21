using NiyaCRM.Core.Common;

namespace NiyaCRM.Core.Tenants;

/// <summary>
/// Service interface for tenant business operations.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="name">The tenant name.</param>
    /// <param name="host">The tenant host.</param>
    /// <param name="email">The tenant email.</param>
    /// <param name="databaseName">The database name (optional).</param>
    /// <param name="createdBy">The user creating the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tenant.</returns>
    Task<Tenant> CreateTenantAsync(string name, string host, string email, string? databaseName = null, string? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by its identifier.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found, otherwise null.</returns>
    Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by its host.
    /// </summary>
    /// <param name="host">The tenant host.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found, otherwise null.</returns>
    Task<Tenant?> GetTenantByHostAsync(string host, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates tenant information.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="name">The new tenant name.</param>
    /// <param name="host">The new tenant host.</param>
    /// <param name="email">The new tenant email.</param>
    /// <param name="databaseName">The new database name (optional).</param>
    /// <param name="modifiedBy">The user updating the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated tenant.</returns>
    Task<Tenant> UpdateTenantAsync(Guid id, string name, string host, string email, string? databaseName = null, string? modifiedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="modifiedBy">The user activating the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activated tenant.</returns>
    Task<Tenant> ActivateTenantAsync(Guid id, string? modifiedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="modifiedBy">The user deactivating the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deactivated tenant.</returns>
    Task<Tenant> DeactivateTenantAsync(Guid id, string? modifiedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active tenants.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active tenants.</returns>
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tenants with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated collection of tenants.</returns>
    Task<IEnumerable<Tenant>> GetAllTenantsAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a tenant host is available.
    /// </summary>
    /// <param name="host">The host to validate.</param>
    /// <param name="excludeId">Optional tenant ID to exclude from validation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the host is available, otherwise false.</returns>
    Task<bool> IsHostAvailableAsync(string host, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
