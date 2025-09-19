using OXDesk.Core.Common;
using OXDesk.Core.Tenants.DTOs;

namespace OXDesk.Core.Tenants;

/// <summary>
/// Service interface for tenant business operations.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="request">The tenant creation request.</param>
    /// <param name="createdBy">The user creating the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tenant.</returns>
    Task<Tenant> CreateTenantAsync(CreateTenantRequest request, Guid? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by its identifier.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found, otherwise null.</returns>
    Task<Tenant?> GetTenantByIdAsync(int id, CancellationToken cancellationToken = default);

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
    /// <param name="request">The tenant update request.</param>
    /// <param name="modifiedBy">The user updating the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated tenant.</returns>
    Task<Tenant> UpdateTenantAsync(int id, UpdateTenantRequest request, Guid? modifiedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any tenants exist in the system.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if at least one tenant exists, otherwise false.</returns>
    Task<bool> AnyTenantsExistAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the activation status of a tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="action">The action to perform (activate or deactivate).</param>
    /// <param name="reason">The reason for the status change.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated tenant.</returns>
    Task<Tenant> ChangeTenantActivationStatusAsync(int id, string action, string reason, CancellationToken cancellationToken = default);

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
    Task<bool> IsHostAvailableAsync(string host, int? excludeId = null, CancellationToken cancellationToken = default);
}
