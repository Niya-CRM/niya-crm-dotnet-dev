namespace NiyaCRM.Core.Tenants;

/// <summary>
/// Repository interface for tenant data access operations.
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Gets a tenant by its unique identifier.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found, otherwise null.</returns>
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by its host.
    /// </summary>
    /// <param name="host">The tenant host.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found, otherwise null.</returns>
    Task<Tenant?> GetByHostAsync(string host, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by its email.
    /// </summary>
    /// <param name="email">The tenant email.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found, otherwise null.</returns>
    Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active tenants.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active tenants.</returns>
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tenants with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of tenants.</returns>
    Task<IEnumerable<Tenant>> GetAllAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new tenant.
    /// </summary>
    /// <param name="tenant">The tenant to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added tenant.</returns>
    Task<Tenant> AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    /// <param name="tenant">The tenant to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated tenant.</returns>
    Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a tenant exists with the specified host.
    /// </summary>
    /// <param name="host">The host to check.</param>
    /// <param name="excludeId">Optional tenant ID to exclude from the check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a tenant exists with the host, otherwise false.</returns>
    Task<bool> ExistsByHostAsync(string host, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a tenant exists with the specified email.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <param name="excludeId">Optional tenant ID to exclude from the check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a tenant exists with the email, otherwise false.</returns>
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
