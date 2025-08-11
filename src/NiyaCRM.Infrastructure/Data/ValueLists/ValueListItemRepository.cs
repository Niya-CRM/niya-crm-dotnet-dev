using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.ValueLists;

namespace NiyaCRM.Infrastructure.Data.ValueLists;

/// <summary>
/// Repository implementation for ValueListItem operations.
/// </summary>
public class ValueListItemRepository : IValueListItemRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ValueListItemRepository> _logger;
    private readonly DbSet<ValueListItem> _dbSet;

    public ValueListItemRepository(ApplicationDbContext dbContext, ILogger<ValueListItemRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(logger);
        _dbContext = dbContext;
        _logger = logger;
        _dbSet = dbContext.Set<ValueListItem>();
    }

    public async Task<IEnumerable<ValueListItem>> GetByValueListIdAsync(Guid valueListId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting ValueListItems for ValueListId: {ValueListId}", valueListId);
        return await _dbSet.Where(v => v.ValueListId == valueListId)
            .OrderBy(v => v.ItemName)
            .ToListAsync(cancellationToken);
    }

    public async Task<ValueListItem> AddAsync(ValueListItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        _logger.LogDebug("Adding ValueListItem: {Name} to ValueList: {ValueListId}", item.ItemName, item.ValueListId);
        if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
        if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        if (item.CreatedBy == Guid.Empty) item.CreatedBy = CommonConstant.DEFAULT_TECHNICAL_USER;
        if (item.UpdatedBy == Guid.Empty) item.UpdatedBy = item.CreatedBy;

        var entry = await _dbSet.AddAsync(item, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Added ValueListItem with ID: {Id}", entry.Entity.Id);
        return entry.Entity;
    }

    public async Task<ValueListItem> UpdateAsync(ValueListItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id == Guid.Empty) throw new ArgumentException("ValueListItem Id cannot be empty.", nameof(item));

        _logger.LogDebug("Updating ValueListItem: {Id}", item.Id);
        item.UpdatedAt = DateTime.UtcNow;
        var entry = _dbSet.Update(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated ValueListItem: {Id}", item.Id);
        return entry.Entity;
    }

    public async Task<ValueListItem> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            _logger.LogWarning("ValueListItem not found for activation: {Id}", id);
            throw new InvalidOperationException($"ValueListItem with ID '{id}' not found.");
        }

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy ?? CommonConstant.DEFAULT_TECHNICAL_USER;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Activated ValueListItem: {Id}", id);
        return entity;
    }

    public async Task<ValueListItem> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            _logger.LogWarning("ValueListItem not found for deactivation: {Id}", id);
            throw new InvalidOperationException($"ValueListItem with ID '{id}' not found.");
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy ?? CommonConstant.DEFAULT_TECHNICAL_USER;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deactivated ValueListItem: {Id}", id);
        return entity;
    }
}
