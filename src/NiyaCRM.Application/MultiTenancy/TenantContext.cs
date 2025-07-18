namespace NiyaCRM.Application.MultiTenancy;

public class TenantContext
{
    private static readonly AsyncLocal<TenantContextHolder> _currentTenant = new();

    public Tenant? CurrentTenant
    {
        get => _currentTenant.Value?.Tenant;
        private set
        {
            if (_currentTenant.Value is null)
            {
                _currentTenant.Value = new TenantContextHolder { Tenant = value };
            }
            else
            {
                _currentTenant.Value.Tenant = value;
            }
        }
    }

    public bool IsSystemAdmin => CurrentTenant is null;

    public void SetCurrentTenant(Tenant? tenant)
    {
        CurrentTenant = tenant;
    }

    private class TenantContextHolder
    {
        public Tenant? Tenant;
    }
}
