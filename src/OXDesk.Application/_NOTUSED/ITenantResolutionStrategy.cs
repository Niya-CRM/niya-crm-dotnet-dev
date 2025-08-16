namespace OXDesk.Application.MultiTenancy;

public interface ITenantResolutionStrategy
{
    string? GetTenantIdentifier();
}
