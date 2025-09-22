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
    private readonly string _valueListLookupCachePrefix = "valuelist:lookup:id:";

    public async Task<IEnumerable<ValueListItem>> GetByListIdAsync(int listId, CancellationToken cancellationToken = default)
    {
        if (listId <= 0) throw new ValidationException("ListId must be a positive integer.");
        _logger.LogDebug("Getting ValueListItems for ListId: {ListId}", listId);
        return await _repository.GetByListIdAsync(listId, cancellationToken);
    }

    public async Task<ValueListItem> CreateAsync(ValueListItem item, int? createdBy = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.ListId <= 0) throw new ValidationException("ListId is required.");
        if (string.IsNullOrWhiteSpace(item.ItemName)) throw new ValidationException("ItemName cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(item.ItemKey)) throw new ValidationException("ItemKey cannot be null or empty.");

        _logger.LogInformation("Creating ValueListItem: {Name} for ValueListId: {ListId}", item.ItemName, item.ListId);

        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        item.CreatedBy = createdBy ?? (item.CreatedBy == 0 ? CommonConstant.DEFAULT_SYSTEM_USER : item.CreatedBy);
        item.UpdatedBy = item.CreatedBy;

        // TenantId will be handled by DbContext based on current tenant scope
        var created = await _repository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created ValueListItem with ID: {Id}", created.Id);
        // Invalidate lookup cache for the list id
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{created.ListId}");
        return created;
    }

    public async Task<ValueListItem> UpdateAsync(ValueListItem item, int? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id <= 0) throw new ValidationException("Id must be a positive integer for update.");
        if (item.ListId <= 0) throw new ValidationException("ListId is required.");
        if (string.IsNullOrWhiteSpace(item.ItemName)) throw new ValidationException("ItemName cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(item.ItemKey)) throw new ValidationException("ItemKey cannot be null or empty.");

        _logger.LogInformation("Updating ValueListItem: {Id}", item.Id);
        
        // We trust repository to track entity by key; simply set audit fields here
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = modifiedBy ?? (item.UpdatedBy == 0 ? CommonConstant.DEFAULT_SYSTEM_USER : item.UpdatedBy);

        var updated = await _repository.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated ValueListItem: {Id}", updated.Id);
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{updated.ListId}");
        return updated;
    }

    public async Task<ValueListItem> ActivateAsync(int id, int? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ValidationException("Id must be a positive integer.");
        _logger.LogInformation("Activating ValueListItem: {Id}", id);
        
        var entity = await _repository.ActivateAsync(id, modifiedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{entity.ListId}");
        return entity;
    }

    public async Task<ValueListItem> DeactivateAsync(int id, int? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (id <= 0) throw new ValidationException("Id must be a positive integer.");
        _logger.LogInformation("Deactivating ValueListItem: {Id}", id);
        
        var entity = await _repository.DeactivateAsync(id, modifiedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"{_valueListLookupCachePrefix}{entity.ListId}");
        return entity;
    }
}

