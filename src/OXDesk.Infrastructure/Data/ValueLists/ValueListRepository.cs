using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Common;
using OXDesk.Core.ValueLists;

namespace OXDesk.Infrastructure.Data.ValueLists;

/// <summary>
/// Repository implementation for ValueList operations.
/// </summary>
public class ValueListRepository : IValueListRepository
{
    private readonly ILogger<ValueListRepository> _logger;
    private readonly DbSet<ValueList> _dbSet;

    public ValueListRepository(TenantDbContext dbContext, ILogger<ValueListRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _dbSet = dbContext.Set<ValueList>();
    }

    public async Task<ValueList?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting ValueList by ID: {Id}", id);
        return await _dbSet
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ValueList?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("ValueList name cannot be null or empty.", nameof(name));

        var trimmed = name.Trim();
        _logger.LogDebug("Getting ValueList by Name: {Name}", trimmed);
        return await _dbSet
            .Where(v => v.ListName == trimmed)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ValueList>> GetAllAsync(
        int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < CommonConstant.PAGE_NUMBER_DEFAULT)
            throw new ArgumentException($"Page number must be >= {CommonConstant.PAGE_NUMBER_DEFAULT}.", nameof(pageNumber));
        if (pageSize < CommonConstant.PAGE_SIZE_MIN || pageSize > CommonConstant.PAGE_SIZE_MAX)
            throw new ArgumentException($"Page size must be between {CommonConstant.PAGE_SIZE_MIN} and {CommonConstant.PAGE_SIZE_MAX}.", nameof(pageSize));

        _logger.LogDebug("Getting ValueLists - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        return await _dbSet
            .OrderBy(v => v.ListName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<ValueList> AddAsync(ValueList valueList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);

        _logger.LogDebug("Adding ValueList: {Name}", valueList.ListName);
        if (valueList.CreatedAt == default) valueList.CreatedAt = DateTime.UtcNow;
        valueList.UpdatedAt = DateTime.UtcNow;

        var entry = await _dbSet.AddAsync(valueList, cancellationToken);
        _logger.LogInformation("Added ValueList with ID: {Id}", entry.Entity.Id);
        return entry.Entity;
    }

    public async Task<ValueList> UpdateAsync(ValueList valueList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);
        if (valueList.Id <= 0) throw new ArgumentException("ValueList Id must be a positive integer.", nameof(valueList));

        _logger.LogDebug("Updating ValueList: {Id}", valueList.Id);
        
        // Verify the entity exists under current tenant scope (global filters apply)
        var existingEntity = await _dbSet
            .Where(v => v.Id == valueList.Id)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"ValueList with ID {valueList.Id} not found.");
        }
        
        valueList.UpdatedAt = DateTime.UtcNow;
        
        var entry = _dbSet.Update(valueList);
        _logger.LogInformation("Updated ValueList: {Id}", valueList.Id);
        return entry.Entity;
    }

    public async Task<ValueList> ActivateAsync(int id, int modifiedBy, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (entity == null)
        {
            _logger.LogWarning("ValueList not found for activation: {Id}", id);
            throw new InvalidOperationException($"ValueList with ID '{id}' not found.");
        }

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy;
        _logger.LogInformation("Activated ValueList: {Id}", id);
        return entity;
    }

    public async Task<ValueList> DeactivateAsync(int id, int modifiedBy, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (entity == null)
        {
            _logger.LogWarning("ValueList not found for deactivation: {Id}", id);
            throw new InvalidOperationException($"ValueList with ID '{id}' not found.");
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy;
        _logger.LogInformation("Deactivated ValueList: {Id}", id);
        return entity;
    }
}
