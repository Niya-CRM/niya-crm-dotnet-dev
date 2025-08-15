using NiyaCRM.Core.Common;

namespace NiyaCRM.Core.ValueLists;

/// <summary>
/// Service interface for ValueList business operations.
/// </summary>
public interface IValueListService
{
    Task<ValueList> CreateAsync(ValueList valueList, Guid? createdBy = null, CancellationToken cancellationToken = default);
    Task<ValueList> UpdateAsync(ValueList valueList, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ValueList>> GetAllAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default);
    Task<ValueList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ValueList?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<ValueList> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<ValueList> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
}
