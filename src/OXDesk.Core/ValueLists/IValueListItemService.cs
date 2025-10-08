namespace OXDesk.Core.ValueLists;

/// <summary>
/// Service interface for ValueListItem business operations.
/// </summary>
public interface IValueListItemService
{
    /// <summary>
    /// Gets value list items by list id.
    /// </summary>
    /// <param name="listId">The list id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of value list items.</returns>
    Task<IEnumerable<ValueListItem>> GetByListIdAsync(int listId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Creates a new value list item.
    /// </summary>
    /// <param name="item">The value list item to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created value list item.</returns>
    /// <remarks>Tenant context is applied by global query filters; TenantId will be set by the DbContext.</remarks>
    Task<ValueListItem> CreateAsync(ValueListItem item, CancellationToken cancellationToken = default);
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activated value list item.</returns>
    Task<ValueListItem> ActivateAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deactivates a value list item.
    /// </summary>
    /// <param name="id">The value list item identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deactivated value list item.</returns>
    Task<ValueListItem> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
