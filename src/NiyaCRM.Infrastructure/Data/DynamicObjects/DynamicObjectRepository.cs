using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NiyaCRM.Core.DynamicObjects;
using NiyaCRM.Core.Common;

namespace NiyaCRM.Infrastructure.Data.DynamicObjects;

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
    public async Task<DynamicObject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting dynamic object by ID: {DynamicObjectId}", id);
        
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObject>> GetAllAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        if (pageNumber < CommonConstant.PAGE_NUMBER_DEFAULT)
            throw new ArgumentException($"Page number must be greater than {CommonConstant.PAGE_NUMBER_DEFAULT}.", nameof(pageNumber));
        
        if (pageSize < CommonConstant.PAGE_SIZE_MIN || pageSize > CommonConstant.PAGE_SIZE_MAX)
            throw new ArgumentException($"Page size must be between {CommonConstant.PAGE_SIZE_MIN} and {CommonConstant.PAGE_SIZE_MAX}.", nameof(pageSize));

        _logger.LogDebug("Getting dynamic objects - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        
        return await _dbSet
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
    public async Task<DynamicObject> UpdateAsync(DynamicObject dynamicObject, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dynamicObject);

        _logger.LogDebug("Updating dynamic object: {DynamicObjectId}", dynamicObject.Id);
        
        var entry = _dbSet.Update(dynamicObject);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully updated dynamic object: {DynamicObjectId}", dynamicObject.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string objectName, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            throw new ArgumentException("Object name cannot be null or empty.", nameof(objectName));

        _logger.LogDebug("Checking if object name exists: {ObjectName}, excluding ID: {ExcludeId}", objectName, excludeId);
        
        var normalizedObjectName = objectName.Trim().ToLowerInvariant();
        var query = _dbSet.Where(o => string.Equals(o.ObjectName.ToLower(), normalizedObjectName, StringComparison.OrdinalIgnoreCase));
        
        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}
