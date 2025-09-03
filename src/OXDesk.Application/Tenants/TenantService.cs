using Microsoft.Extensions.Logging;
using OXDesk.Core;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using OXDesk.Core.Tenants;
using OXDesk.Core.Tenants.DTOs;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using OXDesk.Core.Cache;

namespace OXDesk.Application.Tenants;

/// <summary>
/// Service implementation for tenant management operations.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICacheService _cacheService;
    private readonly ICurrentTenant _tenantContextService;
    private readonly string _tenantCachePrefix = "tenant:";

    /// <summary>
    /// Initializes a new instance of the TenantService.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="tenantContextService">The tenant context service.</param>
    /// <param name="cacheService">The cache service.</param>
    public TenantService(
        IUnitOfWork unitOfWork, 
        ILogger<TenantService> logger, 
        IHttpContextAccessor httpContextAccessor,
        ICurrentTenant tenantContextService,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _tenantContextService = tenantContextService ?? throw new ArgumentNullException(nameof(tenantContextService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    private string GetUserIp() =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

    /// <summary>
    /// Adds an audit log entry for tenant-related actions.
    /// </summary>
    /// <param name="event">The event/action type.</param>
    /// <param name="objectItemId">The ID of the affected entity.</param>
    /// <param name="data">The data/details of the action.</param>
    /// <param name="createdBy">The user who performed the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task AddTenantAuditLogAsync(
        string @event,
        string objectItemId,
        string data,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog(
            id: Guid.CreateVersion7(),
            objectKey: CommonConstant.MODULE_TENANT,
            @event: @event,
            objectItemId: objectItemId,
            ip: GetUserIp(),
            data: data,
            createdBy: createdBy
        );
        await _unitOfWork.GetRepository<IAuditLogRepository>().AddAsync(auditLog, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant> CreateTenantAsync(CreateTenantRequest request, Guid? createdBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating tenant with name: {Name}, host: {Host}, email: {Email}", request.Name, request.Host, request.Email);

        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Tenant Name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.Host))
            throw new ValidationException("Tenant Host cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Tenant Email cannot be null or empty.");

        // Normalize inputs
        var normalizedName = request.Name.Trim();
        var normalizedHost = request.Host.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedDatabaseName = request.DatabaseName?.Trim();

        // Check if host is already taken
        var existingTenant = await _unitOfWork.GetRepository<ITenantRepository>().GetByHostAsync(normalizedHost, cancellationToken);
        if (existingTenant != null)
        {
            _logger.LogWarning("Attempt to create tenant with existing host: {Host}", normalizedHost);
            throw new InvalidOperationException($"A tenant with host '{normalizedHost}' already exists.");
        }

        // Get CurrrentTenant Id from ICurrentTenant
        var currentTenantId = _tenantContextService.Id ?? Guid.CreateVersion7();
        

        // Create new tenant
        var tenant = new Tenant(
            id: currentTenantId,
            name: normalizedName,
            host: normalizedHost,
            email: normalizedEmail,
            userId: request.UserId,
            timeZone: request.TimeZone ?? string.Empty,
            databaseName: normalizedDatabaseName,
            isActive: "Y",
            createdAt: DateTime.UtcNow,
            createdBy: createdBy ?? CommonConstant.DEFAULT_SYSTEM_USER
        );

        // Save tenant
        var createdTenant = await _unitOfWork.GetRepository<ITenantRepository>().AddAsync(tenant, cancellationToken);

        // Insert audit log
        await AddTenantAuditLogAsync(
            CommonConstant.AUDIT_LOG_EVENT_CREATE,
            createdTenant.Id.ToString(),
            $"Tenant created: {{ \"Name\": \"{createdTenant.Name}\", \"Host\": \"{createdTenant.Host}\", \"Email\": \"{createdTenant.Email}\" }}",
            createdBy ?? CommonConstant.DEFAULT_SYSTEM_USER,
            cancellationToken
        );

        // Commit transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created tenant with ID: {TenantId}", createdTenant.Id);
        return createdTenant;
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by ID: {TenantId}", id);
        var cacheKey = $"{_tenantCachePrefix}{id}";
        var cachedTenant = await _cacheService.GetAsync<Tenant>(cacheKey);
        if (cachedTenant != null)
        {
            _logger.LogDebug("Tenant {TenantId} found in cache", id);
            return cachedTenant;
        }
        var tenant = await _unitOfWork.GetRepository<ITenantRepository>().GetByIdAsync(id, cancellationToken);
        if (tenant != null)
        {
            await _cacheService.SetAsync(cacheKey, tenant);
            _logger.LogDebug("Tenant {TenantId} cached", id);
        }
        return tenant;
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetTenantByHostAsync(string host, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty.", nameof(host));

        var normalizedHost = host.Trim().ToLowerInvariant();
        var cacheKey = $"{_tenantCachePrefix}{normalizedHost}";
        _logger.LogDebug("Getting tenant by host: {Host}", normalizedHost);

        var cachedTenant = await _cacheService.GetAsync<Tenant>(cacheKey);
        if (cachedTenant != null)
        {
            _logger.LogDebug("Tenant for host {Host} found in cache", normalizedHost);
            return cachedTenant;
        }

        var tenant = await _unitOfWork.GetRepository<ITenantRepository>().GetByHostAsync(normalizedHost, cancellationToken);
        if (tenant != null)
        {
            await _cacheService.SetAsync(cacheKey, tenant);
            _logger.LogDebug("Tenant for host {Host} cached", normalizedHost);
        }
        return tenant;
    }

    /// <inheritdoc />
    public async Task<Tenant> UpdateTenantAsync(Guid id, UpdateTenantRequest request, Guid? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating tenant {TenantId} with name: {Name}, host: {Host}, email: {Email}, databaseName: {DatabaseName}", id, request.Name, request.Host, request.Email, request.DatabaseName);

        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Tenant name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.Host))
            throw new ValidationException("Tenant host cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Tenant email cannot be null or empty.");

        // Get existing tenant
        var tenant = await _unitOfWork.GetRepository<ITenantRepository>().GetByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for update: {TenantId}", id);
            throw new InvalidOperationException($"Tenant with ID '{id}' not found.");
        }

        // Normalize inputs
        var normalizedName = request.Name.Trim();
        var normalizedHost = request.Host.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedDatabaseName = request.DatabaseName?.Trim();

        // Check if new host conflicts with existing tenant (excluding current tenant)
        if (normalizedHost != tenant.Host)
        {
            var existingTenant = await _unitOfWork.GetRepository<ITenantRepository>().GetByHostAsync(normalizedHost, cancellationToken);
            if (existingTenant != null && existingTenant.Id != id)
            {
                _logger.LogWarning("Attempt to update tenant {TenantId} to existing host: {Host}", id, normalizedHost);
                throw new InvalidOperationException($"A tenant with host '{normalizedHost}' already exists. Current tenant: {id}.");
            }
        }

        // Check if new email conflicts with existing tenant (excluding current tenant)
        if (normalizedEmail != tenant.Email)
        {
            var existingTenant = await _unitOfWork.GetRepository<ITenantRepository>().GetByEmailAsync(normalizedEmail, cancellationToken);
            if (existingTenant != null && existingTenant.Id != id)
            {
                _logger.LogWarning("Attempt to update tenant {TenantId} to existing email: {Email}", id, normalizedEmail);
                throw new InvalidOperationException($"A tenant with email '{normalizedEmail}' already exists. Current tenant: {id}.");
            }
        }

        // Remove cache
        await _cacheService.RemoveAsync($"{_tenantCachePrefix}{tenant.Id}");
        await _cacheService.RemoveAsync($"{_tenantCachePrefix}{tenant.Host}");

        // Update tenant properties
        tenant.Name = normalizedName;
        tenant.Host = normalizedHost;
        tenant.Email = normalizedEmail;
        tenant.UserId = request.UserId;
        tenant.TimeZone = request.TimeZone ?? string.Empty;
        tenant.DatabaseName = normalizedDatabaseName;
        tenant.LastModifiedAt = DateTime.UtcNow;
        tenant.LastModifiedBy = modifiedBy ?? CommonConstant.DEFAULT_SYSTEM_USER;

        // Save changes
        var updatedTenant = await _unitOfWork.GetRepository<ITenantRepository>().UpdateAsync(tenant, cancellationToken);

        // Insert audit log for update
        await AddTenantAuditLogAsync(
            CommonConstant.AUDIT_LOG_EVENT_UPDATE,
            updatedTenant.Id.ToString(),
            $"Tenant updated: {{ \"Name\": \"{updatedTenant.Name}\", \"Host\": \"{updatedTenant.Host}\", \"Email\": \"{updatedTenant.Email}\" }}",
            modifiedBy ?? CommonConstant.DEFAULT_SYSTEM_USER,
            cancellationToken
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated tenant: {TenantId}", id);
        return updatedTenant;
    }

    /// <inheritdoc />
    public async Task<Tenant> ChangeTenantActivationStatusAsync(Guid id, string action, string reason, CancellationToken cancellationToken = default)
    {
        bool isActivating = action.Equals(TenantConstant.ActivationAction.Activate, StringComparison.OrdinalIgnoreCase);
        string actionVerb = isActivating ? "Activating" : "Deactivating";
        
        _logger.LogInformation("{ActionVerb} tenant: {TenantId}", actionVerb, id);

        var tenant = await _unitOfWork.GetRepository<ITenantRepository>().GetByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for {ActionVerb}: {TenantId}", actionVerb.ToLower(), id);
            throw new InvalidOperationException($"Tenant with ID '{id}' not found.");
        }

        // Remove cache
        await _cacheService.RemoveAsync($"{_tenantCachePrefix}{tenant.Id}");
        await _cacheService.RemoveAsync($"{_tenantCachePrefix}{tenant.Host}");

        // Set active status based on action
        tenant.IsActive = isActivating ? "Y" : "N";
        tenant.LastModifiedAt = DateTime.UtcNow;
        tenant.LastModifiedBy = CommonConstant.DEFAULT_SYSTEM_USER;
        var updatedTenant = await _unitOfWork.GetRepository<ITenantRepository>().UpdateAsync(tenant, cancellationToken);

        // Insert audit log for activation/deactivation
        string actionPastTense = isActivating ? "activated" : "deactivated";
        await AddTenantAuditLogAsync(
            CommonConstant.AUDIT_LOG_EVENT_UPDATE,
            updatedTenant.Id.ToString(),
            $"Tenant {actionPastTense}: {{ \"Reason\": \"{reason}\" }}",
            CommonConstant.DEFAULT_SYSTEM_USER,
            cancellationToken
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully {ActionPastTense} tenant: {TenantId}", actionPastTense, id);
        return updatedTenant;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active tenants");
        return await _unitOfWork.GetRepository<ITenantRepository>().GetActiveTenantsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsHostAvailableAsync(string host, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty.", nameof(host));

        var normalizedHost = host.Trim().ToLowerInvariant();
        _logger.LogDebug("Checking host availability: {Host}", normalizedHost);

        var existingTenant = await _unitOfWork.GetRepository<ITenantRepository>().GetByHostAsync(normalizedHost, cancellationToken);
        return existingTenant == null || (excludeId.HasValue && existingTenant.Id == excludeId.Value);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all tenants - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        return await _unitOfWork.GetRepository<ITenantRepository>().GetAllAsync(pageNumber, pageSize, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnyTenantsExistAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if any tenants exist in the system");
        var tenants = await _unitOfWork.GetRepository<ITenantRepository>().GetAllAsync(pageNumber: 1, pageSize: 1, cancellationToken);
        return tenants.Any();
    }
}
