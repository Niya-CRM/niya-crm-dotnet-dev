using Microsoft.Extensions.Logging;
using NiyaCRM.Core;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.ValueLists;
using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Application.ValueLists;

/// <summary>
/// Service implementation for ValueList business operations.
/// </summary>
public class ValueListService(IUnitOfWork unitOfWork, IValueListItemService valueListItemService, ILogger<ValueListService> logger) : IValueListService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IValueListItemService _valueListItemService = valueListItemService ?? throw new ArgumentNullException(nameof(valueListItemService));
    private readonly ILogger<ValueListService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ValueList> CreateAsync(ValueList valueList, Guid? createdBy = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);
        if (string.IsNullOrWhiteSpace(valueList.Name))
            throw new ValidationException("ValueList Name cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.Description))
            throw new ValidationException("ValueList Description cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(valueList.ValueListType))
            throw new ValidationException("ValueList Type cannot be null or empty.");

        _logger.LogInformation("Creating ValueList: {Name}", valueList.Name);

        valueList.Id = valueList.Id == Guid.Empty ? Guid.CreateVersion7() : valueList.Id;
        valueList.CreatedAt = DateTime.UtcNow;
        valueList.UpdatedAt = DateTime.UtcNow;
        valueList.CreatedBy = createdBy ?? (valueList.CreatedBy == Guid.Empty ? CommonConstant.DEFAULT_TECHNICAL_USER : valueList.CreatedBy);
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
        if (string.IsNullOrWhiteSpace(valueList.Name))
            throw new ValidationException("ValueList Name cannot be null or empty.");
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

        existing.Name = valueList.Name.Trim();
        existing.Description = valueList.Description.Trim();
        existing.ValueListType = valueList.ValueListType.Trim();
        existing.IsActive = valueList.IsActive;
        existing.AllowModify = valueList.AllowModify;
        existing.AllowNewItem = valueList.AllowNewItem;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = modifiedBy ?? (valueList.UpdatedBy == Guid.Empty ? CommonConstant.DEFAULT_TECHNICAL_USER : valueList.UpdatedBy);

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

    public async Task<IEnumerable<ValueListItem>> GetCountriesAsync(CancellationToken cancellationToken = default)
        => await GetItemsByListNameAsync("Countries", cancellationToken);

    public async Task<IEnumerable<ValueListItem>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
        => await GetItemsByListNameAsync("Currencies", cancellationToken);

    public async Task<IEnumerable<ValueListItem>> GetUserProfilesAsync(CancellationToken cancellationToken = default)
        => await GetItemsByListNameAsync("User Profiles", cancellationToken);

    private async Task<IEnumerable<ValueListItem>> GetItemsByListNameAsync(string listName, CancellationToken cancellationToken)
    {
        var list = await GetByNameAsync(listName, cancellationToken);
        if (list == null)
        {
            _logger.LogWarning("ValueList not found: {Name}", listName);
            return new List<ValueListItem>(capacity: 0);
        }

        var items = await _valueListItemService.GetByValueListIdAsync(list.Id, cancellationToken);
        return items;
    }
}
