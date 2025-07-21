using Microsoft.Extensions.Logging;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Core.Common;

namespace NiyaCRM.Application.Tenants;

/// <summary>
/// Service implementation for tenant management operations.
/// </summary>
public class TenantService(ITenantRepository tenantRepository, ILogger<TenantService> logger) : ITenantService
{
    private readonly ITenantRepository _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
    private readonly ILogger<TenantService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<Tenant> CreateTenantAsync(string name, string host, string email, string? databaseName = null, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating tenant with name: {Name}, host: {Host}, email: {Email}", name, host, email);

        // Validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Tenant host cannot be null or empty.", nameof(host));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Tenant email cannot be null or empty.", nameof(email));

        // Normalize inputs
        var normalizedName = name.Trim();
        var normalizedHost = host.Trim().ToLowerInvariant();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedDatabaseName = databaseName?.Trim();

        // Check if host is already taken
        var existingTenant = await _tenantRepository.GetByHostAsync(normalizedHost, cancellationToken);
        if (existingTenant != null)
        {
            _logger.LogWarning("Attempt to create tenant with existing host: {Host}", normalizedHost);
            throw new InvalidOperationException($"A tenant with host '{normalizedHost}' already exists.");
        }

        // Create new tenant
        var tenant = new Tenant(
            id: Guid.NewGuid(),
            name: normalizedName,
            host: normalizedHost,
            email: normalizedEmail,
            databaseName: normalizedDatabaseName,
            isActive: true,
            createdAt: DateTime.UtcNow,
            createdBy: createdBy ?? "NiyaCRM"
        );

        // Save to repository
        var createdTenant = await _tenantRepository.AddAsync(tenant, cancellationToken);

        _logger.LogInformation("Successfully created tenant with ID: {TenantId}", createdTenant.Id);
        return createdTenant;
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by ID: {TenantId}", id);
        return await _tenantRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetTenantByHostAsync(string host, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty.", nameof(host));

        var normalizedHost = host.Trim().ToLowerInvariant();
        _logger.LogDebug("Getting tenant by host: {Host}", normalizedHost);
        return await _tenantRepository.GetByHostAsync(normalizedHost, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant> UpdateTenantAsync(Guid id, string name, string host, string email, string? databaseName = null, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating tenant {TenantId} with name: {Name}, host: {Host}, email: {Email}, databaseName: {DatabaseName}", id, name, host, email, databaseName);

        // Validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Tenant host cannot be null or empty.", nameof(host));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Tenant email cannot be null or empty.", nameof(email));

        // Get existing tenant
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for update: {TenantId}", id);
            throw new InvalidOperationException($"Tenant with ID '{id}' not found.");
        }

        // Normalize inputs
        var normalizedName = name.Trim();
        var normalizedHost = host.Trim().ToLowerInvariant();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedDatabaseName = databaseName?.Trim();

        // Check if new host conflicts with existing tenant (excluding current tenant)
        if (normalizedHost != tenant.Host)
        {
            var existingTenant = await _tenantRepository.GetByHostAsync(normalizedHost, cancellationToken);
            if (existingTenant != null && existingTenant.Id != id)
            {
                _logger.LogWarning("Attempt to update tenant {TenantId} to existing host: {Host}", id, normalizedHost);
                throw new InvalidOperationException($"A tenant with host '{normalizedHost}' already exists. Current tenant: {id}.");
            }
        }

        // Check if new email conflicts with existing tenant (excluding current tenant)
        if (normalizedEmail != tenant.Email)
        {
            var existingTenant = await _tenantRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (existingTenant != null && existingTenant.Id != id)
            {
                _logger.LogWarning("Attempt to update tenant {TenantId} to existing email: {Email}", id, normalizedEmail);
                throw new InvalidOperationException($"A tenant with email '{normalizedEmail}' already exists. Current tenant: {id}.");
            }
        }

        // Update tenant properties
        tenant.Name = normalizedName;
        tenant.Host = normalizedHost;
        tenant.Email = normalizedEmail;
        tenant.DatabaseName = normalizedDatabaseName;
        tenant.LastModifiedAt = DateTime.UtcNow;
        tenant.LastModifiedBy = modifiedBy ?? "NiyaCRM";

        // Save changes
        var updatedTenant = await _tenantRepository.UpdateAsync(tenant, cancellationToken);

        _logger.LogInformation("Successfully updated tenant: {TenantId}", id);
        return updatedTenant;
    }

    /// <inheritdoc />
    public async Task<Tenant> ActivateTenantAsync(Guid id, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating tenant: {TenantId}", id);

        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for activation: {TenantId}", id);
            throw new InvalidOperationException($"Tenant with ID '{id}' not found.");
        }

        tenant.IsActive = true;
        tenant.LastModifiedAt = DateTime.UtcNow;
        tenant.LastModifiedBy = modifiedBy ?? "NiyaCRM";
        var updatedTenant = await _tenantRepository.UpdateAsync(tenant, cancellationToken);

        _logger.LogInformation("Successfully activated tenant: {TenantId}", id);
        return updatedTenant;
    }

    /// <inheritdoc />
    public async Task<Tenant> DeactivateTenantAsync(Guid id, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating tenant: {TenantId}", id);

        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for deactivation: {TenantId}", id);
            throw new InvalidOperationException($"Tenant with ID '{id}' not found.");
        }

        tenant.IsActive = false;
        tenant.LastModifiedAt = DateTime.UtcNow;
        tenant.LastModifiedBy = modifiedBy ?? "NiyaCRM";
        var updatedTenant = await _tenantRepository.UpdateAsync(tenant, cancellationToken);

        _logger.LogInformation("Successfully deactivated tenant: {TenantId}", id);
        return updatedTenant;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active tenants");
        return await _tenantRepository.GetActiveTenantsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsHostAvailableAsync(string host, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty.", nameof(host));

        var normalizedHost = host.Trim().ToLowerInvariant();
        _logger.LogDebug("Checking host availability: {Host}", normalizedHost);

        var exists = await _tenantRepository.ExistsByHostAsync(normalizedHost, excludeId, cancellationToken);
        return !exists;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all tenants - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        return await _tenantRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteTenantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting tenant: {TenantId}", id);
        
        var deleted = await _tenantRepository.DeleteAsync(id, cancellationToken);
        
        if (deleted)
        {
            _logger.LogInformation("Successfully deleted tenant: {TenantId}", id);
        }
        else
        {
            _logger.LogWarning("Tenant not found for deletion: {TenantId}", id);
        }
        
        return deleted;
    }
}
