using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.DynamicObjects.Fields;

namespace NiyaCRM.Infrastructure.Data.DynamicObjects.Fields;

/// <summary>
/// Repository implementation for dynamic object fields using Entity Framework Core.
/// </summary>
public class DynamicObjectFieldRepository : IDynamicObjectFieldRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DynamicObjectFieldRepository> _logger;
    private readonly DbSet<DynamicObjectFieldType> _dbSetFieldTypes;
    private readonly DbSet<DynamicObjectField> _dbSetFields;

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
    }

    /// <inheritdoc />
    public async Task<DynamicObjectFieldType?> GetFieldTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting DynamicObjectFieldType by ID: {Id}", id);
        return await _dbSetFieldTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObjectFieldType>> GetAllFieldTypesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting DynamicObjectFieldTypes");

        return await _dbSetFieldTypes.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObjectField>> GetFieldsByObjectKeyAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("Object key cannot be null or empty.", nameof(objectKey));

        var normalizedKey = objectKey.Trim();
        _logger.LogDebug("Getting DynamicObjectFields for ObjectKey: {ObjectKey}", normalizedKey);

        return await _dbSetFields.AsNoTracking()
            .Where(f => f.ObjectKey == normalizedKey && f.DeletedAt == null)
            .OrderBy(f => f.Label)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField?> GetFieldByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting DynamicObjectField by ID: {Id}", id);
        return await _dbSetFields.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id && f.DeletedAt == null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> AddFieldAsync(DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);

        _logger.LogDebug("Adding DynamicObjectField for ObjectKey: {ObjectKey}, FieldKey: {FieldKey}", field.ObjectKey, field.FieldKey);
        var entry = await _dbSetFields.AddAsync(field, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Added DynamicObjectField with ID: {Id}", entry.Entity.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> UpdateFieldAsync(DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);

        _logger.LogDebug("Updating DynamicObjectField ID: {Id}", field.Id);
        var entry = _dbSetFields.Update(field);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated DynamicObjectField ID: {Id}", field.Id);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFieldAsync(string objectKey, Guid fieldId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("Object key cannot be null or empty.", nameof(objectKey));

        var normalizedKey = objectKey.Trim();
        _logger.LogDebug("Deleting DynamicObjectField ID: {Id} for ObjectKey: {ObjectKey}", fieldId, normalizedKey);

        var entity = await _dbSetFields.FirstOrDefaultAsync(f => f.Id == fieldId && f.ObjectKey == normalizedKey, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("DynamicObjectField not found. ID: {Id}, ObjectKey: {ObjectKey}", fieldId, normalizedKey);
            return false;
        }
        // Soft delete: set DeletedAt
        entity.DeletedAt = System.DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted DynamicObjectField ID: {Id} for ObjectKey: {ObjectKey}", fieldId, normalizedKey);
        return true;
    }
}
