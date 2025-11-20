using OXDesk.Core.Common;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.ValueLists.DTOs;
using System;
using System.Collections.Generic;

namespace OXDesk.Core.ValueLists;

/// <summary>
/// Service interface for ValueList business operations.
/// </summary>
public interface IValueListService
{
    /// <summary>
    /// Creates a new value list.
    /// </summary>
    Task<ValueList> CreateAsync(ValueList valueList, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing value list.
    /// </summary>
    Task<ValueList> UpdateAsync(ValueList valueList, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all value lists with pagination.
    /// </summary>
    Task<IEnumerable<ValueList>> GetAllAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a value list by its identifier.
    /// </summary>
    Task<ValueList?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a value list by its name.
    /// </summary>
    Task<ValueList?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Activates a value list.
    /// </summary>
    Task<ValueList> ActivateAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deactivates a value list.
    /// </summary>
    Task<ValueList> DeactivateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets countries as value list item options.
    /// </summary>
    Task<IEnumerable<ValueListItemOption>> GetCountriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets currencies as value list item options.
    /// </summary>
    Task<IEnumerable<ValueListItemOption>> GetCurrenciesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets user profiles as value list item options.
    /// </summary>
    Task<IEnumerable<ValueListItemOption>> GetUserProfilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a dictionary lookup (ItemKey -> ValueListItem) for the specified list name with caching.
    /// </summary>
    Task<IReadOnlyDictionary<string, ValueListItem>> GetLookupByListNameAsync(string listName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets countries as a dictionary lookup with caching.
    /// </summary>
    Task<IReadOnlyDictionary<string, ValueListItem>> GetCountriesLookupAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets currencies as a dictionary lookup with caching.
    /// </summary>
    Task<IReadOnlyDictionary<string, ValueListItem>> GetCurrenciesLookupAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets user profiles as a dictionary lookup with caching.
    /// </summary>
    Task<IReadOnlyDictionary<string, ValueListItem>> GetUserProfilesLookupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a generic dictionary lookup built from cached items with custom key selector.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    /// <param name="listKey">The value list key.</param>
    /// <param name="keySelector">Function to select the key from value list item.</param>
    /// <param name="comparer">Optional equality comparer for keys.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IReadOnlyDictionary<TKey, ValueListItem>> GetLookupAsync<TKey>(
        string listKey,
        Func<ValueListItem, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull;

    /// <summary>
    /// Gets common status options (Active/Inactive).
    /// </summary>
    IEnumerable<StatusOption> GetStatuses();
}
