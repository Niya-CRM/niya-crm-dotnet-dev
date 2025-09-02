using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Common;
using OXDesk.Core.ValueLists;

namespace OXDesk.Infrastructure.Data.ValueLists;

/// <summary>
/// Repository implementation for ValueListItem operations.
/// </summary>
public class ValueListItemRepository : IValueListItemRepository
{
    private readonly ILogger<ValueListItemRepository> _logger;
    private readonly DbSet<ValueListItem> _dbSet;

    public ValueListItemRepository(ApplicationDbContext dbContext, ILogger<ValueListItemRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _dbSet = dbContext.Set<ValueListItem>();
    }

    public async Task<IEnumerable<ValueListItem>> GetByListKeyAsync(string listKey, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting ValueListItems for ListKey: {ListKey}", listKey);
        return await _dbSet
            .Where(v => v.ListKey == listKey)
            .OrderBy(v => v.Order == null) // non-null first
            .ThenBy(v => v.Order)
            .ThenBy(v => v.ItemName)
            .ToListAsync(cancellationToken);
    }

    public async Task<ValueListItem> AddAsync(ValueListItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        _logger.LogDebug("Adding ValueListItem: {Name} to ValueList: {ListKey}", item.ItemName, item.ListKey);
        if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
        if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        if (item.CreatedBy == Guid.Empty) item.CreatedBy = CommonConstant.DEFAULT_SYSTEM_USER;
        if (item.UpdatedBy == Guid.Empty) item.UpdatedBy = item.CreatedBy;

        var entry = await _dbSet.AddAsync(item, cancellationToken);
        _logger.LogInformation("Added ValueListItem with ID: {Id}", entry.Entity.Id);
        return entry.Entity;
    }

    public async Task<ValueListItem> UpdateAsync(ValueListItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id == Guid.Empty) throw new ArgumentException("ValueListItem Id cannot be empty.", nameof(item));

        _logger.LogDebug("Updating ValueListItem: {Id}", item.Id);
        
        // Verify the entity exists under current tenant scope (global filters apply)
        var existingEntity = await _dbSet
            .Where(v => v.Id == item.Id)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"ValueListItem with ID {item.Id} not found.");
        }
        
        item.UpdatedAt = DateTime.UtcNow;
        
        var entry = _dbSet.Update(item);
        _logger.LogInformation("Updated ValueListItem: {Id}", item.Id);
        return entry.Entity;
    }

    public async Task<ValueListItem> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (entity == null)
        {
            _logger.LogWarning("ValueListItem not found for activation: {Id}", id);
            throw new InvalidOperationException($"ValueListItem with ID '{id}' not found.");
        }

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy ?? CommonConstant.DEFAULT_SYSTEM_USER;
        _logger.LogInformation("Activated ValueListItem: {Id}", id);
        return entity;
    }

    public async Task<ValueListItem> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (entity == null)
        {
            _logger.LogWarning("ValueListItem not found for deactivation: {Id}", id);
            throw new InvalidOperationException($"ValueListItem with ID '{id}' not found.");
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy ?? CommonConstant.DEFAULT_SYSTEM_USER;
        _logger.LogInformation("Deactivated ValueListItem: {Id}", id);
        return entity;
    }
}


