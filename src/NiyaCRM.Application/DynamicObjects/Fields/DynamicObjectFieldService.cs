using Microsoft.Extensions.Logging;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.DynamicObjects.Fields;

namespace NiyaCRM.Application.DynamicObjects.Fields;

/// <summary>
/// Service implementation for dynamic object field type operations.
/// </summary>
public class DynamicObjectFieldService : IDynamicObjectFieldService
{
    private readonly IDynamicObjectFieldRepository _repository;
    private readonly ILogger<DynamicObjectFieldService> _logger;

    public DynamicObjectFieldService(
        IDynamicObjectFieldRepository repository,
        ILogger<DynamicObjectFieldService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<DynamicObjectFieldType?> GetFieldTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching DynamicObjectFieldType by ID: {Id}", id);
        return await _repository.GetFieldTypeByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObjectFieldType>> GetAllFieldTypesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all DynamicObjectFieldTypes");
        return await _repository.GetAllFieldTypesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObjectField>> GetFieldsByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default)
    {
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        _logger.LogDebug("Fetching DynamicObjectFields for ObjectId: {ObjectId}", objectId);
        return await _repository.GetFieldsByObjectIdAsync(objectId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField?> GetFieldByIdAsync(Guid objectId, Guid id, CancellationToken cancellationToken = default)
    {
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        _logger.LogDebug("Fetching DynamicObjectField by ID: {Id} for ObjectId: {ObjectId}", id, objectId);
        return await _repository.GetFieldByIdAsync(objectId, id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> AddFieldAsync(Guid objectId, DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        _logger.LogDebug("Adding DynamicObjectField for ObjectId: {ObjectId}, FieldKey: {FieldKey}", objectId, field.FieldKey);
        return await _repository.AddFieldAsync(objectId, field, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> UpdateFieldAsync(Guid objectId, DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        _logger.LogDebug("Updating DynamicObjectField ID: {Id} for ObjectId: {ObjectId}", field.Id, objectId);
        return await _repository.UpdateFieldAsync(objectId, field, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFieldAsync(Guid objectId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        _logger.LogDebug("Deleting DynamicObjectField ID: {Id} for ObjectId: {ObjectId}", fieldId, objectId);
        return await _repository.DeleteFieldAsync(objectId, fieldId, cancellationToken);
    }
}
