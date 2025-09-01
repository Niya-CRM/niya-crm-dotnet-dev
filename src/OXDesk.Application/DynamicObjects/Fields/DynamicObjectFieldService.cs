using Microsoft.Extensions.Logging;
using OXDesk.Core.Common;
using OXDesk.Core.DynamicObjects.Fields;

namespace OXDesk.Application.DynamicObjects.Fields;

/// <summary>
/// Service implementation for dynamic object field type operations.
/// </summary>
public class DynamicObjectFieldService : IDynamicObjectFieldService
{
    private readonly IDynamicObjectFieldRepository _repository;
    private readonly ILogger<DynamicObjectFieldService> _logger;
    private readonly ITenantContextService _tenantContextService;

    public DynamicObjectFieldService(
        IDynamicObjectFieldRepository repository,
        ILogger<DynamicObjectFieldService> logger,
        ITenantContextService tenantContextService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContextService = tenantContextService ?? throw new ArgumentNullException(nameof(tenantContextService));
    }

    /// <inheritdoc />
    public async Task<DynamicObjectFieldType?> GetFieldTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching DynamicObjectFieldType by ID: {Id}", id);
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        return await _repository.GetFieldTypeByIdAsync(id, tenantId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObjectFieldType>> GetAllFieldTypesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all DynamicObjectFieldTypes");
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        return await _repository.GetAllFieldTypesAsync(tenantId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObjectField>> GetFieldsByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default)
    {
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        _logger.LogDebug("Fetching DynamicObjectFields for ObjectId: {ObjectId}", objectId);
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        return await _repository.GetFieldsByObjectIdAsync(objectId, tenantId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField?> GetFieldByIdAsync(Guid objectId, Guid id, CancellationToken cancellationToken = default)
    {
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        _logger.LogDebug("Fetching DynamicObjectField by ID: {Id} for ObjectId: {ObjectId}", id, objectId);
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        return await _repository.GetFieldByIdAsync(objectId, id, tenantId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> AddFieldAsync(Guid objectId, DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        // Set tenant ID on the field entity
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        field.TenantId = tenantId;
        
        _logger.LogDebug("Adding DynamicObjectField for ObjectId: {ObjectId}, FieldKey: {FieldKey}", objectId, field.FieldKey);
        return await _repository.AddFieldAsync(objectId, field, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObjectField> UpdateFieldAsync(Guid objectId, DynamicObjectField field, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        _logger.LogDebug("Updating DynamicObjectField ID: {Id} for ObjectId: {ObjectId}", field.Id, objectId);
        return await _repository.UpdateFieldAsync(objectId, field, tenantId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFieldAsync(Guid objectId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        if (objectId == Guid.Empty)
            throw new ArgumentException("Object ID cannot be empty.", nameof(objectId));

        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        _logger.LogDebug("Deleting DynamicObjectField ID: {Id} for ObjectId: {ObjectId}", fieldId, objectId);
        return await _repository.DeleteFieldAsync(objectId, fieldId, tenantId, cancellationToken);
    }
}
