namespace OXDesk.Application.MultiTenancy;

public interface ITenantService
{
    Task<Tenant?> GetTenantAsync(string identifier);
    Task<bool> SetCurrentTenantAsync(string? identifier);
    Tenant? GetCurrentTenant();
    bool IsSystemAdmin();
}
