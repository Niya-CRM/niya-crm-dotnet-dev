using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Tenants.DTOs;

namespace OXDesk.Core.Tenants;

public interface ITenantFactory
{
    Task<PagedListWithRelatedResponse<TenantResponse>> BuildListAsync(IEnumerable<Tenant> tenants, int pageNumber, CancellationToken cancellationToken = default);

    Task<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>> BuildDetailsAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
