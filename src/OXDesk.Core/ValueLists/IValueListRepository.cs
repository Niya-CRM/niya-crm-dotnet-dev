using OXDesk.Core.Common;

namespace OXDesk.Core.ValueLists;

/// <summary>
/// Repository interface for ValueList persistence operations.
/// </summary>
public interface IValueListRepository
{
    /// <summary>
    /// Gets a value list by its identifier.
    /// </summary>
    /// <param name="id">The value list identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value list if found; otherwise, null.</returns>
    Task<ValueList?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value list by its display name.
    /// </summary>
    /// <param name="name">The value list name.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value list if found; otherwise, null.</returns>
    Task<ValueList?> GetByNameAsync(string name, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value list by its unique list key.
    /// </summary>
    /// <param name="key">The value list key.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value list if found; otherwise, null.</returns>
    Task<ValueList?> GetByKeyAsync(string key, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all value lists with pagination.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of value lists.</returns>
    Task<IEnumerable<ValueList>> GetAllAsync(
        Guid tenantId,
        int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new value list.
    /// </summary>
    /// <param name="valueList">The value list to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added value list.</returns>
    /// <remarks>TenantId should be set on the valueList entity before calling this method.</remarks>
    Task<ValueList> AddAsync(ValueList valueList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing value list.
    /// </summary>
    /// <param name="valueList">The value list to update.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated value list.</returns>
    Task<ValueList> UpdateAsync(ValueList valueList, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates the value list.
    /// </summary>
    /// <param name="id">The value list identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="modifiedBy">The user who modified the value list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activated value list.</returns>
    Task<ValueList> ActivateAsync(Guid id, Guid tenantId, Guid? modifiedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates the value list.
    /// </summary>
    /// <param name="id">The value list identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="modifiedBy">The user who modified the value list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deactivated value list.</returns>
    Task<ValueList> DeactivateAsync(Guid id, Guid tenantId, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
}
