namespace OXDesk.Core.ValueLists;

/// <summary>
/// Service interface for ValueListItem business operations.
/// </summary>
public interface IValueListItemService
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
    /// Creates a new value list item.
    /// </summary>
    /// <param name="item">The value list item to create.</param>
    /// <param name="createdBy">The user who created the item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created value list item.</returns>
    /// <remarks>TenantId should be set on the item entity before calling this method.</remarks>
    Task<ValueListItem> CreateAsync(ValueListItem item, Guid? createdBy = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an existing value list item.
    /// </summary>
    /// <param name="item">The value list item to update.</param>
    /// <param name="modifiedBy">The user who modified the item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated value list item.</returns>
    Task<ValueListItem> UpdateAsync(ValueListItem item, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Activates a value list item.
    /// </summary>
    /// <param name="id">The value list item identifier.</param>
    /// <param name="modifiedBy">The user who modified the item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activated value list item.</returns>
    Task<ValueListItem> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deactivates a value list item.
    /// </summary>
    /// <param name="id">The value list item identifier.</param>
    /// <param name="modifiedBy">The user who modified the item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deactivated value list item.</returns>
    Task<ValueListItem> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
}
