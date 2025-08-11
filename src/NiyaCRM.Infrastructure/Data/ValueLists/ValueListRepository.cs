using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.ValueLists;

namespace NiyaCRM.Infrastructure.Data.ValueLists;

/// <summary>
/// Repository implementation for ValueList operations.
/// </summary>
public class ValueListRepository : IValueListRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ValueListRepository> _logger;
    private readonly DbSet<ValueList> _dbSet;

    public ValueListRepository(ApplicationDbContext dbContext, ILogger<ValueListRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(logger);
        _dbContext = dbContext;
        _logger = logger;
        _dbSet = dbContext.Set<ValueList>();
    }

    public async Task<ValueList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting ValueList by ID: {Id}", id);
        return await _dbSet.FindAsync([id], cancellationToken);
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
            .OrderBy(v => v.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<ValueList> AddAsync(ValueList valueList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);

        _logger.LogDebug("Adding ValueList: {Name}", valueList.Name);
        if (valueList.Id == Guid.Empty) valueList.Id = Guid.NewGuid();
        if (valueList.CreatedAt == default) valueList.CreatedAt = DateTime.UtcNow;
        valueList.UpdatedAt = DateTime.UtcNow;
        if (valueList.CreatedBy == Guid.Empty) valueList.CreatedBy = CommonConstant.DEFAULT_TECHNICAL_USER;
        if (valueList.UpdatedBy == Guid.Empty) valueList.UpdatedBy = valueList.CreatedBy;

        var entry = await _dbSet.AddAsync(valueList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Added ValueList with ID: {Id}", entry.Entity.Id);
        return entry.Entity;
    }

    public async Task<ValueList> UpdateAsync(ValueList valueList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);
        if (valueList.Id == Guid.Empty) throw new ArgumentException("ValueList Id cannot be empty.", nameof(valueList));

        _logger.LogDebug("Updating ValueList: {Id}", valueList.Id);
        valueList.UpdatedAt = DateTime.UtcNow;
        var entry = _dbSet.Update(valueList);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated ValueList: {Id}", valueList.Id);
        return entry.Entity;
    }

    public async Task<ValueList> ActivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            _logger.LogWarning("ValueList not found for activation: {Id}", id);
            throw new InvalidOperationException($"ValueList with ID '{id}' not found.");
        }

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy ?? CommonConstant.DEFAULT_TECHNICAL_USER;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Activated ValueList: {Id}", id);
        return entity;
    }

    public async Task<ValueList> DeactivateAsync(Guid id, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            _logger.LogWarning("ValueList not found for deactivation: {Id}", id);
            throw new InvalidOperationException($"ValueList with ID '{id}' not found.");
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy ?? CommonConstant.DEFAULT_TECHNICAL_USER;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deactivated ValueList: {Id}", id);
        return entity;
    }
}
