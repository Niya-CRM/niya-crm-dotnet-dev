using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.Common;

namespace OXDesk.Infrastructure.Data.DynamicObjects;

/// <summary>
/// Repository implementation for dynamic object data access operations using Entity Framework.
/// </summary>
public class DynamicObjectRepository : IDynamicObjectRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DynamicObjectRepository> _logger;
    private readonly DbSet<DynamicObject> _dbSet;

    /// <summary>
    /// Initializes a new instance of the DynamicObjectRepository.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    public DynamicObjectRepository(ApplicationDbContext dbContext, ILogger<DynamicObjectRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(logger);
        
        _dbContext = dbContext;
        _logger = logger;
        _dbSet = dbContext.Set<DynamicObject>();
    }

    /// <inheritdoc />
    public async Task<DynamicObject?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting dynamic object by ID: {DynamicObjectId}, TenantId: {TenantId}", id, tenantId);
        
        return await _dbSet
            .Where(o => o.Id == id && o.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObject>> GetAllAsync(Guid tenantId, int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        if (pageNumber < CommonConstant.PAGE_NUMBER_DEFAULT)
            throw new ArgumentException($"Page number must be greater than {CommonConstant.PAGE_NUMBER_DEFAULT}.", nameof(pageNumber));
        
        if (pageSize < CommonConstant.PAGE_SIZE_MIN || pageSize > CommonConstant.PAGE_SIZE_MAX)
            throw new ArgumentException($"Page size must be between {CommonConstant.PAGE_SIZE_MIN} and {CommonConstant.PAGE_SIZE_MAX}.", nameof(pageSize));

        _logger.LogDebug("Getting dynamic objects - TenantId: {TenantId}, Page: {PageNumber}, Size: {PageSize}", tenantId, pageNumber, pageSize);
        
        return await _dbSet
            .Where(o => o.TenantId == tenantId)
            .OrderBy(o => o.ObjectName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObject> AddAsync(DynamicObject dynamicObject, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dynamicObject);

        _logger.LogDebug("Adding new dynamic object: {ObjectName}", dynamicObject.ObjectName);
        
        var entry = await _dbSet.AddAsync(dynamicObject, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully added dynamic object with ID: {DynamicObjectId}", dynamicObject.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<DynamicObject> UpdateAsync(DynamicObject dynamicObject, Guid tenantId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dynamicObject);

        _logger.LogDebug("Updating dynamic object: {DynamicObjectId}, TenantId: {TenantId}", dynamicObject.Id, tenantId);
        
        // Verify the entity belongs to the specified tenant
        var existingEntity = await _dbSet
            .Where(o => o.Id == dynamicObject.Id && o.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"Dynamic object with ID {dynamicObject.Id} not found for tenant {tenantId}");
        }
        
        // Ensure tenant ID is preserved
        dynamicObject.TenantId = tenantId;
        
        var entry = _dbSet.Update(dynamicObject);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully updated dynamic object: {DynamicObjectId}", dynamicObject.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string objectName, Guid tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            throw new ArgumentException("Object name cannot be null or empty.", nameof(objectName));

        _logger.LogDebug("Checking if object name exists: {ObjectName}, TenantId: {TenantId}, excluding ID: {ExcludeId}", objectName, tenantId, excludeId);
        
        var normalizedObjectName = objectName.Trim().ToLowerInvariant();
        var query = _dbSet
            .Where(o => o.TenantId == tenantId)
            .Where(o => string.Equals(o.ObjectName.ToLower(), normalizedObjectName, StringComparison.OrdinalIgnoreCase));
        
        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}
