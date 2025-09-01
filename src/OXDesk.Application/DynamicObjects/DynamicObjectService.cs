using Microsoft.Extensions.Logging;
using OXDesk.Core;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.DynamicObjects.DTOs;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using OXDesk.Core.Cache;
using OXDesk.Application.Common;

namespace OXDesk.Application.DynamicObjects;

/// <summary>
/// Service implementation for dynamic object management operations.
/// </summary>
public class DynamicObjectService : IDynamicObjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DynamicObjectService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICacheService _cacheService;
    private readonly ITenantContextService _tenantContextService;

    private readonly string _dynamicObjectCachePrefix = "dynamic_object:";

    /// <summary>
    /// Initializes a new instance of the DynamicObjectService.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="tenantContextService">The tenant context service.</param>
    public DynamicObjectService(
        IUnitOfWork unitOfWork, 
        ILogger<DynamicObjectService> logger, 
        IHttpContextAccessor httpContextAccessor, 
        ICacheService cacheService,
        ITenantContextService tenantContextService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _tenantContextService = tenantContextService ?? throw new ArgumentNullException(nameof(tenantContextService));
    }

    private string GetUserIp() =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

    /// <summary>
    /// Adds an audit log entry for dynamic object-related actions.
    /// </summary>
    /// <param name="event">The event/action type.</param>
    /// <param name="objectItemId">The ID of the affected entity.</param>
    /// <param name="data">The data/details of the action.</param>
    /// <param name="createdBy">The user who performed the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task AddDynamicObjectAuditLogAsync(
        string @event,
        string objectItemId,
        string data,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        // Get tenant ID from the tenant context service
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        
        var auditLog = new AuditLog(
            id: Guid.CreateVersion7(),
            tenantId: tenantId,
            objectKey: "dynamic_object",
            @event: @event,
            objectItemId: objectItemId,
            ip: GetUserIp(),
            data: data,
            createdBy: createdBy
        );
        await _unitOfWork.GetRepository<IAuditLogRepository>().AddAsync(auditLog, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DynamicObject> CreateDynamicObjectAsync(DynamicObjectRequest request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating dynamic object with name: {ObjectName}", request.ObjectName);

        // Validation
        if (string.IsNullOrWhiteSpace(request.ObjectName))
            throw new ValidationException("Object Name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.SingularName))
            throw new ValidationException("Singular Name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.PluralName))
            throw new ValidationException("Plural Name cannot be null or empty.");

        // Normalize inputs
        var normalizedObjectName = request.ObjectName.Trim();
        var normalizedSingularName = request.SingularName.Trim();
        var normalizedPluralName = request.PluralName.Trim();
        var normalizedDescription = request.Description?.Trim() ?? string.Empty;
        
        // Generate object key from object name (only letters with __c suffix)
        var normalizedObjectKey = new string(normalizedObjectName
            .ToLowerInvariant()
            .Where(c => char.IsLetter(c))
            .ToArray()) + "__c";

        // Get tenant ID from the tenant context service
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        
        // Check if object name is already taken
        var exists = await _unitOfWork.GetRepository<IDynamicObjectRepository>().ExistsByNameAsync(normalizedObjectName, tenantId, null, cancellationToken);
        if (exists)
        {
            _logger.LogWarning("Attempt to create dynamic object with existing name: {ObjectName}", normalizedObjectName);
            throw new InvalidOperationException($"A dynamic object with name '{normalizedObjectName}' already exists.");
        }

        // Create new dynamic object
        var dynamicObject = new DynamicObject(
            id: Guid.CreateVersion7(),
            objectName: normalizedObjectName,
            singularName: normalizedSingularName,
            pluralName: normalizedPluralName,
            objectKey: normalizedObjectKey,
            description: normalizedDescription,
            objectType: DynamicObjectConstants.ObjectTypes.Custom,
            createdBy: createdBy
        );

        // Save dynamic object
        var createdDynamicObject = await _unitOfWork.GetRepository<IDynamicObjectRepository>().AddAsync(dynamicObject, cancellationToken);

        // Insert audit log
        await AddDynamicObjectAuditLogAsync(
            CommonConstant.AUDIT_LOG_EVENT_CREATE,
            createdDynamicObject.Id.ToString(),
            $"Dynamic object created: {{ \"ObjectName\": \"{createdDynamicObject.ObjectName}\", \"ObjectKey\": \"{createdDynamicObject.ObjectKey}\" }}",
            createdBy,
            cancellationToken
        );

        // Commit transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created dynamic object with ID: {DynamicObjectId}", createdDynamicObject.Id);
        return createdDynamicObject;
    }

    /// <inheritdoc />
    public async Task<DynamicObject?> GetDynamicObjectByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting dynamic object by ID: {DynamicObjectId}", id);
        var cacheKey = $"{_dynamicObjectCachePrefix}{id}";
        var cachedDynamicObject = await _cacheService.GetAsync<DynamicObject>(cacheKey);
        if (cachedDynamicObject != null)
        {
            _logger.LogDebug("Dynamic object {DynamicObjectId} found in cache", id);
            return cachedDynamicObject;
        }
        // Get tenant ID from the tenant context service
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        var dynamicObject = await _unitOfWork.GetRepository<IDynamicObjectRepository>().GetByIdAsync(id, tenantId, cancellationToken);
        if (dynamicObject != null)
        {
            await _cacheService.SetAsync(cacheKey, dynamicObject);
            _logger.LogDebug("Dynamic object {DynamicObjectId} cached", id);
        }
        return dynamicObject;
    }

    /// <inheritdoc />
    public async Task<DynamicObject> UpdateDynamicObjectAsync(Guid id, DynamicObjectRequest request, Guid modifiedBy, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating dynamic object {DynamicObjectId} with name: {ObjectName}", id, request.ObjectName);

        // Validation
        if (string.IsNullOrWhiteSpace(request.ObjectName))
            throw new ValidationException("Object name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.SingularName))
            throw new ValidationException("Singular name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.PluralName))
            throw new ValidationException("Plural name cannot be null or empty.");

        // Get tenant ID from the tenant context service
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        
        // Get existing dynamic object
        var dynamicObject = await _unitOfWork.GetRepository<IDynamicObjectRepository>().GetByIdAsync(id, tenantId, cancellationToken);
        if (dynamicObject == null)
        {
            _logger.LogWarning("Dynamic object not found for update: {DynamicObjectId}", id);
            throw new InvalidOperationException($"Dynamic object with ID '{id}' not found.");
        }

        // Normalize inputs
        var normalizedObjectName = request.ObjectName.Trim();
        var normalizedSingularName = request.SingularName.Trim();
        var normalizedPluralName = request.PluralName.Trim();
        var normalizedDescription = request.Description?.Trim() ?? string.Empty;

        // Check if new object name conflicts with existing dynamic object (excluding current object)
        if (normalizedObjectName != dynamicObject.ObjectName)
        {
            var exists = await _unitOfWork.GetRepository<IDynamicObjectRepository>().ExistsByNameAsync(normalizedObjectName, tenantId, id, cancellationToken);
            if (exists)
            {
                _logger.LogWarning("Attempt to update dynamic object {DynamicObjectId} to existing name: {ObjectName}", id, normalizedObjectName);
                throw new InvalidOperationException($"A dynamic object with name '{normalizedObjectName}' already exists.");
            }
        }

        // Remove cache
        await _cacheService.RemoveAsync($"{_dynamicObjectCachePrefix}{dynamicObject.Id}");

        // Update dynamic object properties
        dynamicObject.ObjectName = normalizedObjectName;
        dynamicObject.SingularName = normalizedSingularName;
        dynamicObject.PluralName = normalizedPluralName;
        dynamicObject.Description = normalizedDescription;
        dynamicObject.UpdatedAt = DateTime.UtcNow;
        dynamicObject.UpdatedBy = modifiedBy;

        // Save changes
        var updatedDynamicObject = await _unitOfWork.GetRepository<IDynamicObjectRepository>().UpdateAsync(dynamicObject, tenantId, cancellationToken);

        // Insert audit log for update
        await AddDynamicObjectAuditLogAsync(
            CommonConstant.AUDIT_LOG_EVENT_UPDATE,
            updatedDynamicObject.Id.ToString(),
            $"Dynamic object updated: {{ \"ObjectName\": \"{updatedDynamicObject.ObjectName}\", \"ObjectKey\": \"{updatedDynamicObject.ObjectKey}\" }}",
            modifiedBy,
            cancellationToken
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated dynamic object: {DynamicObjectId}", id);
        return updatedDynamicObject;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DynamicObject>> GetAllDynamicObjectsAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all dynamic objects - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        // Get tenant ID from the tenant context service
        Guid tenantId = _tenantContextService.GetCurrentTenantId();
        return await _unitOfWork.GetRepository<IDynamicObjectRepository>().GetAllAsync(tenantId, pageNumber, pageSize, cancellationToken);
    }
}
