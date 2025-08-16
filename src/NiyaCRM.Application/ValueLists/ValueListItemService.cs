using Microsoft.Extensions.Logging;
using NiyaCRM.Core;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.ValueLists;
using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Application.ValueLists;

/// <summary>
/// Service implementation for ValueListItem business operations.
/// </summary>
public class ValueListItemService(IUnitOfWork unitOfWork, ILogger<ValueListItemService> logger) : IValueListItemService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<ValueListItemService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<ValueListItem>> GetByListKeyAsync(string listKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(listKey)) throw new ValidationException("ListKey is required.");
        var trimmed = listKey.Trim();
        _logger.LogDebug("Getting ValueListItems for ListKey: {ListKey}", trimmed);
        return await _unitOfWork.GetRepository<IValueListItemRepository>().GetByListKeyAsync(trimmed, cancellationToken);
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
        item.CreatedBy = createdBy ?? (item.CreatedBy == Guid.Empty ? CommonConstant.DEFAULT_TECHNICAL_USER : item.CreatedBy);
        item.UpdatedBy = item.CreatedBy;

        var created = await _unitOfWork.GetRepository<IValueListItemRepository>().AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created ValueListItem with ID: {Id}", created.Id);
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

        var existingItemsRepo = _unitOfWork.GetRepository<IValueListItemRepository>();
        // We trust repository to track entity by key; simply set audit fields here
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = modifiedBy ?? (item.UpdatedBy == Guid.Empty ? CommonConstant.DEFAULT_TECHNICAL_USER : item.UpdatedBy);

        var updated = await existingItemsRepo.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated ValueListItem: {Id}", updated.Id);
        return updated;
    }

    public async Task<ValueListItem> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Id is required.");
        _logger.LogInformation("Activating ValueListItem: {Id}", id);
        var entity = await _unitOfWork.GetRepository<IValueListItemRepository>().ActivateAsync(id, modifiedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ValueListItem> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ValidationException("Id is required.");
        _logger.LogInformation("Deactivating ValueListItem: {Id}", id);
        var entity = await _unitOfWork.GetRepository<IValueListItemRepository>().DeactivateAsync(id, modifiedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }
}

