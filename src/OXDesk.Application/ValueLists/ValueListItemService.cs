using Microsoft.Extensions.Logging;
using OXDesk.Core.Common;
using OXDesk.Core.ValueLists;
using System.ComponentModel.DataAnnotations;
using OXDesk.Core;
using OXDesk.Core.Cache;

namespace OXDesk.Application.ValueLists;

/// <summary>
/// Service implementation for ValueListItem business operations.
/// </summary>
public class ValueListItemService(IValueListItemRepository repository, IUnitOfWork unitOfWork, ICacheService cacheService, ILogger<ValueListItemService> logger) : IValueListItemService
{
    private readonly IValueListItemRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<ValueListItemService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string _valueListLookupCachePrefix = "valuelist:lookup:";

    public async Task<IEnumerable<ValueListItem>> GetByListKeyAsync(string listKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(listKey)) throw new ValidationException("ListKey is required.");
        var trimmed = listKey.Trim();
        _logger.LogDebug("Getting ValueListItems for ListKey: {ListKey}", trimmed);
        
        return await _repository.GetByListKeyAsync(trimmed, cancellationToken);
    }

    public async Task<ValueListItem> CreateAsync(ValueListItem item, Guid? createdBy = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (string.IsNullOrWhiteSpace(item.ListKey)) throw new ValidationException("ListKey is required.");
        if (string.IsNullOrWhiteSpace(item.ItemName)) throw new ValidationException("ItemName cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(item.ItemKey)) throw new ValidationException("ItemKey cannot be null or empty.");

        _logger.LogInformation("Creating ValueListItem: {Name} for ValueList: {ListKey}", item.ItemName, item.ListKey);
        
        item.Id = item.Id == Guid.Empty ? Guid.CreateVersion7() : item.Id;
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        item.CreatedBy = createdBy ?? (item.CreatedBy == Guid.Empty ? CommonConstant.DEFAULT_SYSTEM_USER : item.CreatedBy);
        item.UpdatedBy = item.CreatedBy;

        // TenantId will be handled by DbContext based on current tenant scope
        var created = await _repository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created ValueListItem with ID: {Id}", created.Id);
        // Invalidate lookup cache for the list key
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{created.ListKey}");
        return created;
    }

    public async Task<ValueListItem> UpdateAsync(ValueListItem item, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id == Guid.Empty) throw new ValidationException("Id is required for update.");
        if (string.IsNullOrWhiteSpace(item.ListKey)) throw new ValidationException("ListKey is required.");
        if (string.IsNullOrWhiteSpace(item.ItemName)) throw new ValidationException("ItemName cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(item.ItemKey)) throw new ValidationException("ItemKey cannot be null or empty.");

        _logger.LogInformation("Updating ValueListItem: {Id}", item.Id);
        
        // We trust repository to track entity by key; simply set audit fields here
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = modifiedBy ?? (item.UpdatedBy == Guid.Empty ? CommonConstant.DEFAULT_SYSTEM_USER : item.UpdatedBy);

        var updated = await _repository.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated ValueListItem: {Id}", updated.Id);
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{updated.ListKey}");
        return updated;
    }

    public async Task<ValueListItem> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Id is required.");
        _logger.LogInformation("Activating ValueListItem: {Id}", id);
        
        var entity = await _repository.ActivateAsync(id, modifiedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{entity.ListKey}");
        return entity;
    }

    public async Task<ValueListItem> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Id is required.");
        _logger.LogInformation("Deactivating ValueListItem: {Id}", id);
        
        var entity = await _repository.DeactivateAsync(id, modifiedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{entity.ListKey}");
        return entity;
    }
}

