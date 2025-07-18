using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NiyaCRM.Application.MultiTenancy;

public class TenantService : ITenantService
{
    private readonly TenantContext _tenantContext;
    private readonly ITenantResolutionStrategy _tenantResolutionStrategy;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        TenantContext tenantContext,
        ITenantResolutionStrategy tenantResolutionStrategy,
        ITenantRepository tenantRepository,
        ILogger<TenantService> logger)
    {
        _tenantContext = tenantContext;
        _tenantResolutionStrategy = tenantResolutionStrategy;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task<Tenant?> GetTenantAsync(string identifier)
    {
        return await _tenantRepository.GetByIdentifierAsync(identifier);
    }

    public Tenant? GetCurrentTenant()
    {
        return _tenantContext.CurrentTenant;
    }

    public bool IsSystemAdmin()
    {
        return _tenantContext.IsSystemAdmin;
    }

    public async Task<bool> SetCurrentTenantAsync(string? identifier)
    {
        try
        {
            if (string.IsNullOrEmpty(identifier))
            {
                _tenantContext.SetCurrentTenant(null);
                return true;
            }
            var tenant = await GetTenantAsync(identifier);
            if (tenant == null)
            {
                _logger.LogWarning("No tenant found for identifier: {Identifier}", identifier);
                return false;
            }
            if (!tenant.IsActive)
            {
                _logger.LogWarning("Tenant is not active: {Identifier}", identifier);
                return false;
            }
            _tenantContext.SetCurrentTenant(tenant);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting current tenant for identifier: {Identifier}", identifier);
            return false;
        }
    }
}
