namespace NiyaCRM.Core.DynamicObjects;

/// <summary>
/// Repository interface for dynamic object data access operations.
/// </summary>
public interface IDynamicObjectRepository
{
    /// <summary>
    /// Gets a dynamic object by its unique identifier.
    /// </summary>
    /// <param name="id">The dynamic object identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dynamic object if found, otherwise null.</returns>
    Task<DynamicObject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all dynamic objects with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of dynamic objects.</returns>
    Task<IEnumerable<DynamicObject>> GetAllAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new dynamic object.
    /// </summary>
    /// <param name="dynamicObject">The dynamic object to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added dynamic object.</returns>
    Task<DynamicObject> AddAsync(DynamicObject dynamicObject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing dynamic object.
    /// </summary>
    /// <param name="dynamicObject">The dynamic object to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated dynamic object.</returns>
    Task<DynamicObject> UpdateAsync(DynamicObject dynamicObject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a dynamic object exists with the specified name.
    /// </summary>
    /// <param name="objectName">The object name to check.</param>
    /// <param name="excludeId">Optional dynamic object ID to exclude from the check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a dynamic object exists with the name, otherwise false.</returns>
    Task<bool> ExistsByNameAsync(string objectName, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
