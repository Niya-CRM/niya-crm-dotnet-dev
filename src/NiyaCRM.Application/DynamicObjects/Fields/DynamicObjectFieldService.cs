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
    public async Task<IEnumerable<DynamicObjectField>> GetFieldsByObjectKeyAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("Object key cannot be null or empty.", nameof(objectKey));

        var normalizedKey = objectKey.Trim();
        _logger.LogDebug("Fetching DynamicObjectFields for ObjectKey: {ObjectKey}", normalizedKey);
        return await _repository.GetFieldsByObjectKeyAsync(normalizedKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField?> GetFieldByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching DynamicObjectField by ID: {Id}", id);
        return await _repository.GetFieldByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> AddFieldAsync(DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        _logger.LogDebug("Adding DynamicObjectField for ObjectKey: {ObjectKey}, FieldKey: {FieldKey}", field.ObjectKey, field.FieldKey);
        return await _repository.AddFieldAsync(field, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> UpdateFieldAsync(DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        _logger.LogDebug("Updating DynamicObjectField ID: {Id}", field.Id);
        return await _repository.UpdateFieldAsync(field, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFieldAsync(string objectKey, Guid fieldId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("Object key cannot be null or empty.", nameof(objectKey));

        var normalizedKey = objectKey.Trim();
        _logger.LogDebug("Deleting DynamicObjectField ID: {Id} for ObjectKey: {ObjectKey}", fieldId, normalizedKey);
        return await _repository.DeleteFieldAsync(normalizedKey, fieldId, cancellationToken);
    }
}
