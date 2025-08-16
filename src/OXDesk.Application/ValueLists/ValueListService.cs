using Microsoft.Extensions.Logging;
using OXDesk.Core;
using OXDesk.Core.Common;
using OXDesk.Core.ValueLists;
using OXDesk.Core.Cache;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using OXDesk.Core.Common.DTOs;

namespace OXDesk.Application.ValueLists;

/// <summary>
/// Service implementation for ValueList business operations.
/// </summary>
public class ValueListService(IUnitOfWork unitOfWork, IValueListItemService valueListItemService, ILogger<ValueListService> logger, ICacheService cacheService) : IValueListService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IValueListItemService _valueListItemService = valueListItemService ?? throw new ArgumentNullException(nameof(valueListItemService));
    private readonly ILogger<ValueListService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly string _valueListLookupCachePrefix = "valuelist:lookup:";

    public async Task<ValueList> CreateAsync(ValueList valueList, Guid? createdBy = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);
        if (string.IsNullOrWhiteSpace(valueList.ListName))
            throw new ValidationException("ValueList ListName cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.ListKey))
            throw new ValidationException("ValueList ListKey cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.Description))
            throw new ValidationException("ValueList Description cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.ValueListType))
            throw new ValidationException("ValueList Type cannot be null or empty.");

        _logger.LogInformation("Creating ValueList: {Name}", valueList.ListName);

        valueList.Id = valueList.Id == Guid.Empty ? Guid.CreateVersion7() : valueList.Id;
        valueList.CreatedAt = DateTime.UtcNow;
        valueList.UpdatedAt = DateTime.UtcNow;
        valueList.CreatedBy = createdBy ?? (valueList.CreatedBy == Guid.Empty ? CommonConstant.DEFAULT_SYSTEM_USER : valueList.CreatedBy);
        valueList.UpdatedBy = valueList.CreatedBy;

        var created = await _unitOfWork.GetRepository<IValueListRepository>().AddAsync(valueList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created ValueList with ID: {Id}", created.Id);
        return created;
    }

    public async Task<ValueList> UpdateAsync(ValueList valueList, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);
        if (valueList.Id == Guid.Empty)
            throw new ValidationException("ValueList Id is required for update.");
        if (string.IsNullOrWhiteSpace(valueList.ListName))
            throw new ValidationException("ValueList ListName cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.ListKey))
            throw new ValidationException("ValueList ListKey cannot be null or empty.");
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
        existing.ListKey = valueList.ListKey.Trim();
        existing.Description = valueList.Description.Trim();
        existing.ValueListType = valueList.ValueListType.Trim();
        existing.IsActive = valueList.IsActive;
        existing.AllowModify = valueList.AllowModify;
        existing.AllowNewItem = valueList.AllowNewItem;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = modifiedBy ?? (valueList.UpdatedBy == Guid.Empty ? CommonConstant.DEFAULT_SYSTEM_USER : valueList.UpdatedBy);

        var updated = await _unitOfWork.GetRepository<IValueListRepository>().UpdateAsync(existing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated ValueList: {Id}", updated.Id);
        return updated;
    }

    public async Task<IEnumerable<ValueList>> GetAllAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting ValueLists - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        return await _unitOfWork.GetRepository<IValueListRepository>().GetAllAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<ValueList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting ValueList by ID: {Id}", id);
        return await _unitOfWork.GetRepository<IValueListRepository>().GetByIdAsync(id, cancellationToken);
    }

    public async Task<ValueList?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("ValueList Name cannot be null or empty.");

        var trimmed = name.Trim();
        _logger.LogDebug("Getting ValueList by Name: {Name}", trimmed);
        return await _unitOfWork.GetRepository<IValueListRepository>().GetByNameAsync(trimmed, cancellationToken);
    }

    public async Task<ValueList?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ValidationException("ValueList Key cannot be null or empty.");

        var trimmed = key.Trim();
        _logger.LogDebug("Getting ValueList by Key: {Key}", trimmed);
        return await _unitOfWork.GetRepository<IValueListRepository>().GetByKeyAsync(trimmed, cancellationToken);
    }

    public async Task<ValueList> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating ValueList: {Id}", id);
        var entity = await _unitOfWork.GetRepository<IValueListRepository>().ActivateAsync(id, modifiedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ValueList> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating ValueList: {Id}", id);
        var entity = await _unitOfWork.GetRepository<IValueListRepository>().DeactivateAsync(id, modifiedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private async Task<IEnumerable<ValueListItem>> GetItemsByListKeyAsync(string listKey, CancellationToken cancellationToken)
    {
        // Use the dictionary cache as the single source of truth to avoid double storing.
        var lookup = await GetLookupByListKeyAsync(listKey, cancellationToken);
        return lookup.Values;
    }

    public async Task<IReadOnlyDictionary<string, ValueListItem>> GetLookupByListKeyAsync(string listKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(listKey))
            throw new ValidationException("ListKey cannot be null or empty.");

        var cacheKey = $"{_valueListLookupCachePrefix}{listKey}";
        var cached = await _cacheService.GetAsync<Dictionary<string, ValueListItem>>(cacheKey);
        if (cached != null)
            return cached;

        // Populate dictionary directly from repository to avoid circular calls and double caching.
        var list = await GetByKeyAsync(listKey, cancellationToken);
        if (list == null)
        {
            _logger.LogWarning("ValueList not found: {Key}", listKey);
            return new Dictionary<string, ValueListItem>(capacity: 0, comparer: StringComparer.OrdinalIgnoreCase);
        }
        var items = await _valueListItemService.GetByListKeyAsync(list.ListKey, cancellationToken);
        var dict = items
            .Where(i => !string.IsNullOrWhiteSpace(i.ItemKey))
            .GroupBy(i => i.ItemKey!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        await _cacheService.SetAsync(cacheKey, dict);
        return dict;
    }

    public async Task<IReadOnlyDictionary<string, ValueListItem>> GetCountriesLookupAsync(CancellationToken cancellationToken = default)
        => await GetLookupByListKeyAsync(CommonConstant.ValueListKeys.Countries, cancellationToken);

    public async Task<IReadOnlyDictionary<string, ValueListItem>> GetCurrenciesLookupAsync(CancellationToken cancellationToken = default)
        => await GetLookupByListKeyAsync(CommonConstant.ValueListKeys.Currencies, cancellationToken);

    public async Task<IReadOnlyDictionary<string, ValueListItem>> GetUserProfilesLookupAsync(CancellationToken cancellationToken = default)
        => await GetLookupByListKeyAsync(CommonConstant.ValueListKeys.UserProfiles, cancellationToken);

    public async Task<IEnumerable<ValueListItem>> GetCountriesAsync(CancellationToken cancellationToken = default)
        => await GetItemsByListKeyAsync(CommonConstant.ValueListKeys.Countries, cancellationToken);

    public async Task<IEnumerable<ValueListItem>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
        => await GetItemsByListKeyAsync(CommonConstant.ValueListKeys.Currencies, cancellationToken);

    public async Task<IEnumerable<ValueListItem>> GetUserProfilesAsync(CancellationToken cancellationToken = default)
        => await GetItemsByListKeyAsync(CommonConstant.ValueListKeys.UserProfiles, cancellationToken);

    public async Task<IReadOnlyDictionary<TKey, ValueListItem>> GetLookupAsync<TKey>(
        string listKey,
        Func<ValueListItem, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        if (string.IsNullOrWhiteSpace(listKey))
            throw new ValidationException("ListKey cannot be null or empty.");
        ArgumentNullException.ThrowIfNull(keySelector);

        // Build from already-cached items (single source of truth), no extra caching.
        var items = await GetItemsByListKeyAsync(listKey, cancellationToken);
        var dict = comparer != null
            ? items.ToDictionary(keySelector, i => i, comparer)
            : items.ToDictionary(keySelector, i => i);
        return dict;
    }

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