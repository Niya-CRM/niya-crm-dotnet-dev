using NiyaCRM.Core.Common;
using NiyaCRM.Core.Common.DTOs;
using System;
using System.Collections.Generic;

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
    Task<ValueList?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<ValueList> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<ValueList> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default);

    // Convenience helpers to retrieve common lists and their items
    Task<IEnumerable<ValueListItem>> GetCountriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ValueListItem>> GetCurrenciesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ValueListItem>> GetUserProfilesAsync(CancellationToken cancellationToken = default);

    // Dictionary lookups (ItemKey -> ValueListItem) with caching
    Task<IReadOnlyDictionary<string, ValueListItem>> GetLookupByListKeyAsync(string listKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, ValueListItem>> GetCountriesLookupAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, ValueListItem>> GetCurrenciesLookupAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, ValueListItem>> GetUserProfilesLookupAsync(CancellationToken cancellationToken = default);

    // Generic lookup (e.g., by ItemKey, Id, etc.) built from cached items; avoids double caching
    Task<IReadOnlyDictionary<TKey, ValueListItem>> GetLookupAsync<TKey>(
        string listKey,
        Func<ValueListItem, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull;

    // Common reusable options
    IEnumerable<StatusOption> GetStatuses();
}
