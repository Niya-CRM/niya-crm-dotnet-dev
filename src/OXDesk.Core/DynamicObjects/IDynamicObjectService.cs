using OXDesk.Core.Common;
using OXDesk.Core.DynamicObjects.DTOs;

namespace OXDesk.Core.DynamicObjects;

/// <summary>
/// Service interface for dynamic object business operations.
/// </summary>
public interface IDynamicObjectService
{
    /// <summary>
    /// Creates a new dynamic object.
    /// </summary>
    /// <param name="request">The dynamic object creation request.</param>
    /// <param name="createdBy">The user creating the dynamic object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created dynamic object.</returns>
    Task<DynamicObject> CreateDynamicObjectAsync(DynamicObjectRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a dynamic object by its identifier.
    /// </summary>
    /// <param name="id">The dynamic object identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dynamic object if found, otherwise null.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<DynamicObject?> GetDynamicObjectByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates dynamic object information.
    /// </summary>
    /// <param name="id">The dynamic object identifier.</param>
    /// <param name="request">The dynamic object update request.</param>
    /// <param name="modifiedBy">The user updating the dynamic object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated dynamic object.</returns>
    Task<DynamicObject> UpdateDynamicObjectAsync(Guid id, DynamicObjectRequest request, Guid modifiedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all dynamic objects with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated collection of dynamic objects.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<IEnumerable<DynamicObject>> GetAllDynamicObjectsAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default);
}
