namespace NiyaCRM.Application.MultiTenancy;

public interface ITenantResolutionStrategy
{
    string? GetTenantIdentifier();
}
