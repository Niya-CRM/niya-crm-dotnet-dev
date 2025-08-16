namespace NiyaCRM.Core.ValueLists;

/// <summary>
/// Service interface for ValueListItem business operations.
/// </summary>
public interface IValueListItemService
{
    Task<IEnumerable<ValueListItem>> GetByListKeyAsync(string listKey, CancellationToken cancellationToken = default);
    Task<ValueListItem> CreateAsync(ValueListItem item, Guid? createdBy = null, CancellationToken cancellationToken = default);
    Task<ValueListItem> UpdateAsync(ValueListItem item, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<ValueListItem> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<ValueListItem> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
}
