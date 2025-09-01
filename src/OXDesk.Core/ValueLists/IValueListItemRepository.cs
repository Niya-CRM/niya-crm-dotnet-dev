namespace OXDesk.Core.ValueLists;

/// <summary>
/// Repository interface for ValueListItem persistence operations.
/// </summary>
public interface IValueListItemRepository
{
    /// <summary>
    /// Gets value list items by list key.
    /// </summary>
    /// <param name="listKey">The list key.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of value list items.</returns>
    Task<IEnumerable<ValueListItem>> GetByListKeyAsync(string listKey, Guid tenantId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds a new value list item.
    /// </summary>
    /// <param name="item">The value list item to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added value list item.</returns>
    /// <remarks>TenantId should be set on the item entity before calling this method.</remarks>
    Task<ValueListItem> AddAsync(ValueListItem item, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an existing value list item.
    /// </summary>
    /// <param name="item">The value list item to update.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated value list item.</returns>
    Task<ValueListItem> UpdateAsync(ValueListItem item, Guid tenantId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Activates a value list item.
    /// </summary>
    /// <param name="id">The value list item identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="modifiedBy">The user who modified the item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activated value list item.</returns>
    Task<ValueListItem> ActivateAsync(Guid id, Guid tenantId, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deactivates a value list item.
    /// </summary>
    /// <param name="id">The value list item identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="modifiedBy">The user who modified the item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deactivated value list item.</returns>
    Task<ValueListItem> DeactivateAsync(Guid id, Guid tenantId, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
}
