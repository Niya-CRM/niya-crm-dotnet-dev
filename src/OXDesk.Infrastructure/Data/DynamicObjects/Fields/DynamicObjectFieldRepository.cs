using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Common;
using OXDesk.Core.DynamicObjects.Fields;
using OXDesk.Core.DynamicObjects;

namespace OXDesk.Infrastructure.Data.DynamicObjects.Fields;

/// <summary>
/// Repository implementation for dynamic object fields using Entity Framework Core.
/// </summary>
public class DynamicObjectFieldRepository : IDynamicObjectFieldRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DynamicObjectFieldRepository> _logger;
    private readonly DbSet<DynamicObjectFieldType> _dbSetFieldTypes;
    private readonly DbSet<DynamicObjectField> _dbSetFields;
    private readonly DbSet<DynamicObject> _dbSetObjects;

    public DynamicObjectFieldRepository(
        ApplicationDbContext dbContext,
        ILogger<DynamicObjectFieldRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(logger);

        _dbContext = dbContext;
        _logger = logger;
        _dbSetFieldTypes = dbContext.Set<DynamicObjectFieldType>();
        _dbSetFields = dbContext.Set<DynamicObjectField>();
        _dbSetObjects = dbContext.Set<DynamicObject>();
    }

    /// <inheritdoc />
    public async Task<DynamicObjectFieldType?> GetFieldTypeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting DynamicObjectFieldType by ID: {Id}", id);
        // Note: If DynamicObjectFieldType doesn't have TenantId property, we just filter by ID
        return await _dbSetFieldTypes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObjectFieldType>> GetAllFieldTypesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting DynamicObjectFieldTypes");

        // Note: If DynamicObjectFieldType doesn't have TenantId property, we don't filter by tenant
        return await _dbSetFieldTypes.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObjectField>> GetFieldsByObjectIdAsync(int objectId, CancellationToken cancellationToken = default)
    {
        if (objectId <= 0)
            throw new ArgumentException("Object ID must be a positive integer.", nameof(objectId));

        var obj = await _dbSetObjects.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == objectId && o.DeletedAt == null, cancellationToken);
        if (obj is null)
            throw new InvalidOperationException($"Dynamic object with ID '{objectId}' not found.");

        var objectKey = obj.ObjectKey.Trim();
        _logger.LogDebug("Getting DynamicObjectFields for ObjectId: {ObjectId} (ObjectKey: {ObjectKey})", 
            objectId, objectKey);

        return await _dbSetFields.AsNoTracking()
            .Where(f => f.ObjectKey == objectKey && f.DeletedAt == null)
            .OrderBy(f => f.Label)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField?> GetFieldByIdAsync(int objectId, int id, CancellationToken cancellationToken = default)
    {
        if (objectId <= 0)
            throw new ArgumentException("Object ID must be a positive integer.", nameof(objectId));

        var obj = await _dbSetObjects.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == objectId && o.DeletedAt == null, cancellationToken);
        if (obj is null)
            return null;

        var objectKey = obj.ObjectKey.Trim();
        _logger.LogDebug("Getting DynamicObjectField by ID: {Id} for ObjectId: {ObjectId} (ObjectKey: {ObjectKey})", 
            id, objectId, objectKey);
        return await _dbSetFields.AsNoTracking().FirstOrDefaultAsync(
            f => f.Id == id && f.ObjectKey == objectKey && f.DeletedAt == null,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> AddFieldAsync(int objectId, DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (objectId <= 0)
            throw new ArgumentException("Object ID must be a positive integer.", nameof(objectId));

        var obj = await _dbSetObjects.AsNoTracking().FirstOrDefaultAsync(o => o.Id == objectId && o.DeletedAt == null, cancellationToken);
        if (obj is null)
            throw new InvalidOperationException($"Dynamic object with ID '{objectId}' not found.");

        field.ObjectKey = obj.ObjectKey.Trim();
        _logger.LogDebug("Adding DynamicObjectField for ObjectId: {ObjectId} (ObjectKey: {ObjectKey}), FieldKey: {FieldKey}", objectId, field.ObjectKey, field.FieldKey);
        var entry = await _dbSetFields.AddAsync(field, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Added DynamicObjectField with ID: {Id}", entry.Entity.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> UpdateFieldAsync(int objectId, DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (objectId <= 0)
            throw new ArgumentException("Object ID must be a positive integer.", nameof(objectId));

        var obj = await _dbSetObjects.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == objectId && o.DeletedAt == null, cancellationToken);
        if (obj is null)
            throw new InvalidOperationException($"Dynamic object with ID '{objectId}' not found.");

        field.ObjectKey = obj.ObjectKey.Trim();
        _logger.LogDebug("Updating DynamicObjectField ID: {Id} for ObjectId: {ObjectId} (ObjectKey: {ObjectKey})", 
            field.Id, objectId, field.ObjectKey);
        var entry = _dbSetFields.Update(field);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated DynamicObjectField ID: {Id}", field.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFieldAsync(int objectId, int fieldId, CancellationToken cancellationToken = default)
    {
        if (objectId <= 0)
            throw new ArgumentException("Object ID must be a positive integer.", nameof(objectId));

        var obj = await _dbSetObjects.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == objectId && o.DeletedAt == null, cancellationToken);
        if (obj is null)
            return false;

        var objectKey = obj.ObjectKey.Trim();
        _logger.LogDebug("Deleting DynamicObjectField ID: {Id} for ObjectId: {ObjectId} (ObjectKey: {ObjectKey})", 
            fieldId, objectId, objectKey);

        var entity = await _dbSetFields.FirstOrDefaultAsync(f => f.Id == fieldId && f.ObjectKey == objectKey, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("DynamicObjectField not found. ID: {Id}, ObjectId: {ObjectId}", 
                fieldId, objectId);
            return false;
        }
        // Soft delete: set DeletedAt
        entity.DeletedAt = System.DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted DynamicObjectField ID: {Id} for ObjectId: {ObjectId}", 
            fieldId, objectId);
        return true;
    }
}
