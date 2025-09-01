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

    public async Task<ValueList?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting ValueList by ID: {Id}, TenantId: {TenantId}", id, tenantId);
        return await _dbSet
            .Where(v => v.Id == id && v.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ValueList?> GetByNameAsync(string name, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("ValueList name cannot be null or empty.", nameof(name));

        var trimmed = name.Trim();
        _logger.LogDebug("Getting ValueList by Name: {Name}, TenantId: {TenantId}", trimmed, tenantId);
        return await _dbSet
            .Where(v => v.ListName == trimmed && v.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ValueList?> GetByKeyAsync(string key, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("ValueList key cannot be null or empty.", nameof(key));

        var trimmed = key.Trim();
        _logger.LogDebug("Getting ValueList by Key: {Key}, TenantId: {TenantId}", trimmed, tenantId);
        return await _dbSet
            .Where(v => v.ListKey == trimmed && v.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ValueList>> GetAllAsync(
        Guid tenantId,
        int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < CommonConstant.PAGE_NUMBER_DEFAULT)
            throw new ArgumentException($"Page number must be >= {CommonConstant.PAGE_NUMBER_DEFAULT}.", nameof(pageNumber));
        if (pageSize < CommonConstant.PAGE_SIZE_MIN || pageSize > CommonConstant.PAGE_SIZE_MAX)
            throw new ArgumentException($"Page size must be between {CommonConstant.PAGE_SIZE_MIN} and {CommonConstant.PAGE_SIZE_MAX}.", nameof(pageSize));

        _logger.LogDebug("Getting ValueLists - TenantId: {TenantId}, Page: {PageNumber}, Size: {PageSize}", tenantId, pageNumber, pageSize);

        return await _dbSet
            .Where(v => v.TenantId == tenantId)
            .OrderBy(v => v.ListName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<ValueList> AddAsync(ValueList valueList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);

        _logger.LogDebug("Adding ValueList: {Name}", valueList.ListName);
        if (valueList.Id == Guid.Empty) valueList.Id = Guid.NewGuid();
        if (valueList.CreatedAt == default) valueList.CreatedAt = DateTime.UtcNow;
        valueList.UpdatedAt = DateTime.UtcNow;
        if (valueList.CreatedBy == Guid.Empty) valueList.CreatedBy = CommonConstant.DEFAULT_SYSTEM_USER;
        if (valueList.UpdatedBy == Guid.Empty) valueList.UpdatedBy = valueList.CreatedBy;

        var entry = await _dbSet.AddAsync(valueList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Added ValueList with ID: {Id}", entry.Entity.Id);
        return entry.Entity;
    }

    public async Task<ValueList> UpdateAsync(ValueList valueList, Guid tenantId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(valueList);
        if (valueList.Id == Guid.Empty) throw new ArgumentException("ValueList Id cannot be empty.", nameof(valueList));

        _logger.LogDebug("Updating ValueList: {Id}, TenantId: {TenantId}", valueList.Id, tenantId);
        
        // Verify the entity belongs to the specified tenant
        var existingEntity = await _dbSet
            .Where(v => v.Id == valueList.Id && v.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"ValueList with ID {valueList.Id} not found for tenant {tenantId}");
        }
        
        // Ensure tenant ID is preserved
        valueList.TenantId = tenantId;
        valueList.UpdatedAt = DateTime.UtcNow;
        
        var entry = _dbSet.Update(valueList);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated ValueList: {Id}", valueList.Id);
        return entry.Entity;
    }

    public async Task<ValueList> ActivateAsync(Guid id, Guid tenantId, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .Where(v => v.Id == id && v.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (entity == null)
        {
            _logger.LogWarning("ValueList not found for activation: {Id}, TenantId: {TenantId}", id, tenantId);
            throw new InvalidOperationException($"ValueList with ID '{id}' not found for tenant {tenantId}.");
        }

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy ?? CommonConstant.DEFAULT_SYSTEM_USER;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Activated ValueList: {Id}, TenantId: {TenantId}", id, tenantId);
        return entity;
    }

    public async Task<ValueList> DeactivateAsync(Guid id, Guid tenantId, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .Where(v => v.Id == id && v.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (entity == null)
        {
            _logger.LogWarning("ValueList not found for deactivation: {Id}, TenantId: {TenantId}", id, tenantId);
            throw new InvalidOperationException($"ValueList with ID '{id}' not found for tenant {tenantId}.");
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = modifiedBy ?? CommonConstant.DEFAULT_SYSTEM_USER;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deactivated ValueList: {Id}, TenantId: {TenantId}", id, tenantId);
        return entity;
    }
}
