namespace OXDesk.Core.ValueLists;

/// <summary>
/// Repository interface for ValueListItem persistence operations.
/// </summary>
public interface IValueListItemRepository
{
    Task<IEnumerable<ValueListItem>> GetByListKeyAsync(string listKey, CancellationToken cancellationToken = default);
    Task<ValueListItem> AddAsync(ValueListItem item, CancellationToken cancellationToken = default);
    Task<ValueListItem> UpdateAsync(ValueListItem item, CancellationToken cancellationToken = default);
    Task<ValueListItem> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<ValueListItem> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
}
