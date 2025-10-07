namespace OXDesk.Core.ValueLists;

/// <summary>
/// Repository interface for ValueListItem persistence operations.
/// </summary>
public interface IValueListItemRepository
{
    /// <summary>
    /// Gets value list items by list id.
    /// </summary>
    /// <param name="listId">The list id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of value list items.</returns>
    Task<IEnumerable<ValueListItem>> GetByListIdAsync(int listId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds a new value list item.
    /// </summary>
    /// <param name="item">The value list item to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added value list item.</returns>
    /// <remarks>Tenant context is applied by global query filters; TenantId will be handled by the DbContext.</remarks>
    Task<ValueListItem> AddAsync(ValueListItem item, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an existing value list item.
    /// </summary>
    /// <param name="item">The value list item to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated value list item.</returns>
    Task<ValueListItem> UpdateAsync(ValueListItem item, CancellationToken cancellationToken = default);
    /// <summary>
    /// Activates a value list item.
    /// </summary>
    /// <param name="id">The value list item identifier.</param>
    /// <param name="modifiedBy">The user who modified the item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activated value list item.</returns>
    Task<ValueListItem> ActivateAsync(int id, Guid modifiedBy, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deactivates a value list item.
    /// </summary>
    /// <param name="id">The value list item identifier.</param>
    /// <param name="modifiedBy">The user who modified the item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deactivated value list item.</returns>
    Task<ValueListItem> DeactivateAsync(int id, Guid modifiedBy, CancellationToken cancellationToken = default);
}
