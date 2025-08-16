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
    Task<ValueList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value list by its display name.
    /// </summary>
    Task<ValueList?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value list by its unique list key.
    /// </summary>
    Task<ValueList?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all value lists with pagination.
    /// </summary>
    Task<IEnumerable<ValueList>> GetAllAsync(
        int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new value list.
    /// </summary>
    Task<ValueList> AddAsync(ValueList valueList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing value list.
    /// </summary>
    Task<ValueList> UpdateAsync(ValueList valueList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates the value list.
    /// </summary>
    Task<ValueList> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates the value list.
    /// </summary>
    Task<ValueList> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
}
