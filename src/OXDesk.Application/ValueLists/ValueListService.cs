using Microsoft.Extensions.Logging;
using OXDesk.Core;
using OXDesk.Core.Common;
using OXDesk.Core.ValueLists;
using OXDesk.Core.Cache;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.ValueLists.DTOs;
using OXDesk.Core.Identity;

namespace OXDesk.Application.ValueLists;

/// <summary>
/// Service implementation for ValueList business operations.
/// </summary>
public class ValueListService(IUnitOfWork unitOfWork, IValueListItemService valueListItemService, ICurrentUser currentUser, ILogger<ValueListService> logger, ICacheService cacheService) : IValueListService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IValueListItemService _valueListItemService = valueListItemService ?? throw new ArgumentNullException(nameof(valueListItemService));
    private readonly ICurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly ILogger<ValueListService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly string _valueListLookupCachePrefix = "valuelist:lookup:id:";

    private int GetCurrentUserId()
    {
        return _currentUser.Id ?? throw new InvalidOperationException("Current user ID is null.");
    }

    /// <inheritdoc />
    public async Task<ValueList> CreateAsync(ValueList valueList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);
        if (string.IsNullOrWhiteSpace(valueList.ListName))
            throw new ValidationException("ValueList ListName cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.Description))
            throw new ValidationException("ValueList Description cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.ValueListType))
            throw new ValidationException("ValueList Type cannot be null or empty.");

        _logger.LogInformation("Creating ValueList: {Name}", valueList.ListName);
        
        var currentUserId = GetCurrentUserId();
        valueList.CreatedAt = DateTime.UtcNow;
        valueList.UpdatedAt = DateTime.UtcNow;
        valueList.CreatedBy = currentUserId;
        valueList.UpdatedBy = currentUserId;

        var created = await _unitOfWork.GetRepository<IValueListRepository>().AddAsync(valueList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created ValueList with ID: {Id}", created.Id);
        // Invalidate lookup cache for this list by id
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{created.Id}");
        return created;
    }

    /// <inheritdoc />
    public async Task<ValueList> UpdateAsync(ValueList valueList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);
        if (valueList.Id <= 0)
            throw new ValidationException("ValueList Id must be a positive integer for update.");
        if (string.IsNullOrWhiteSpace(valueList.ListName))
            throw new ValidationException("ValueList ListName cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.Description))
            throw new ValidationException("ValueList Description cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.ValueListType))
            throw new ValidationException("ValueList Type cannot be null or empty.");

        _logger.LogInformation("Updating ValueList: {Id}", valueList.Id);
        var existing = await _unitOfWork.GetRepository<IValueListRepository>().GetByIdAsync(valueList.Id, cancellationToken);
        if (existing == null)
        {
            _logger.LogWarning("ValueList not found for update: {Id}", valueList.Id);
            throw new InvalidOperationException($"ValueList with ID '{valueList.Id}' not found.");
        }

        existing.ListName = valueList.ListName.Trim();
        existing.Description = valueList.Description.Trim();
        existing.ValueListType = valueList.ValueListType.Trim();
        existing.IsActive = valueList.IsActive;
        existing.AllowModify = valueList.AllowModify;
        existing.AllowNewItem = valueList.AllowNewItem;
        existing.UpdatedAt = DateTime.UtcNow;
        var currentUserId = GetCurrentUserId();
        existing.UpdatedBy = currentUserId;
        var updated = await _unitOfWork.GetRepository<IValueListRepository>().UpdateAsync(existing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated ValueList: {Id}", updated.Id);
        // Invalidate lookup cache by id
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{updated.Id}");
        return updated;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ValueList>> GetAllAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting ValueLists - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        return await _unitOfWork.GetRepository<IValueListRepository>().GetAllAsync(pageNumber, pageSize, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ValueList?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting ValueList by ID: {Id}", id);
        return await _unitOfWork.GetRepository<IValueListRepository>().GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ValueList?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("ValueList Name cannot be null or empty.");

        var trimmed = name.Trim();
        _logger.LogDebug("Getting ValueList by Name: {Name}", trimmed);
        return await _unitOfWork.GetRepository<IValueListRepository>().GetByNameAsync(trimmed, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ValueList> ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating ValueList: {Id}", id);
        var currentUserId = GetCurrentUserId();
        var entity = await _unitOfWork.GetRepository<IValueListRepository>().ActivateAsync(id, currentUserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{entity.Id}");
        return entity;
    }

    /// <inheritdoc />
    public async Task<ValueList> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating ValueList: {Id}", id);
        var currentUserId = GetCurrentUserId();
        var entity = await _unitOfWork.GetRepository<IValueListRepository>().DeactivateAsync(id, currentUserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{entity.Id}");
        return entity;
    }

    private async Task<IEnumerable<ValueListItem>> GetItemsByListNameAsync(string listName, CancellationToken cancellationToken)
    {
        // Map key to list name, resolve ValueList, then fetch items by ListId
        var list = await GetByNameAsync(listName, cancellationToken);
        if (list == null) return Array.Empty<ValueListItem>();
        return await _valueListItemService.GetByListIdAsync(list.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, ValueListItem>> GetLookupByListNameAsync(string listName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(listName))
            throw new ValidationException("ListName cannot be null or empty.");
        // Cache by list id to avoid string key reliance
        var list = await GetByNameAsync(listName, cancellationToken);
        if (list == null)
        {
            _logger.LogWarning("ValueList not found: {listName}", listName);
            return new Dictionary<string, ValueListItem>(capacity: 0, comparer: StringComparer.OrdinalIgnoreCase);
        }
        var cacheKey = $"{_valueListLookupCachePrefix}{list.Id}";
        var cached = await _cacheService.GetAsync<Dictionary<string, ValueListItem>>(cacheKey);
        if (cached != null)
            return cached;

        // Populate dictionary directly from repository to avoid circular calls and double caching.
        var items = await _valueListItemService.GetByListIdAsync(list.Id, cancellationToken);
        var dict = items
            .Where(i => !string.IsNullOrWhiteSpace(i.ItemKey))
            .GroupBy(i => i.ItemKey!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        await _cacheService.SetAsync(cacheKey, dict);
        return dict;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, ValueListItem>> GetCountriesLookupAsync(CancellationToken cancellationToken = default)
        => await GetLookupByListNameAsync(CommonConstant.ValueListNames.Countries, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, ValueListItem>> GetCurrenciesLookupAsync(CancellationToken cancellationToken = default)
        => await GetLookupByListNameAsync(CommonConstant.ValueListNames.Currencies, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, ValueListItem>> GetUserProfilesLookupAsync(CancellationToken cancellationToken = default)
        => await GetLookupByListNameAsync(CommonConstant.ValueListNames.UserProfiles, cancellationToken);

    /// <inheritdoc />
    public async Task<IEnumerable<ValueListItemOption>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        var items = await GetItemsByListNameAsync(CommonConstant.ValueListNames.Countries, cancellationToken);
        return items
            .Select(i => new ValueListItemOption { Id = i.Id, ItemName = i.ItemName, ItemKey = i.ItemKey, IsActive = i.IsActive, Order = i.Order });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ValueListItemOption>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        var items = await GetItemsByListNameAsync(CommonConstant.ValueListNames.Currencies, cancellationToken);
        return items
            .Select(i => new ValueListItemOption { Id = i.Id, ItemName = i.ItemName, ItemKey = i.ItemKey, IsActive = i.IsActive, Order = i.Order });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ValueListItemOption>> GetUserProfilesAsync(CancellationToken cancellationToken = default)
    {
        var items = await GetItemsByListNameAsync(CommonConstant.ValueListNames.UserProfiles, cancellationToken);
        return items
            .Select(i => new ValueListItemOption { Id = i.Id, ItemName = i.ItemName, ItemKey = i.ItemKey, IsActive = i.IsActive, Order = i.Order });
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<TKey, ValueListItem>> GetLookupAsync<TKey>(
        string listName,
        Func<ValueListItem, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        if (string.IsNullOrWhiteSpace(listName))
            throw new ValidationException("ListName cannot be null or empty.");
        ArgumentNullException.ThrowIfNull(keySelector);

        // Build from already-cached items (single source of truth), no extra caching.
        var items = await GetItemsByListNameAsync(listName, cancellationToken);
        var dict = comparer != null
            ? items.ToDictionary(keySelector, i => i, comparer)
            : items.ToDictionary(keySelector, i => i);
        return dict;
    }

    /// <inheritdoc />
    public IEnumerable<StatusOption> GetStatuses()
    {
        // Static reusable options for boolean statuses
        return new[]
        {
            new StatusOption { Name = "Active", Value = true },
            new StatusOption { Name = "Inactive", Value = false }
        };
    }
}