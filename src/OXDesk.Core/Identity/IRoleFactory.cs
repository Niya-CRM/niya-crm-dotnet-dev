using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Core.Identity
{
    /// <summary>
    /// Factory interface for building role DTOs and response wrappers, including enrichment and related data.
    /// Located in Core so controllers can depend on this abstraction.
    /// </summary>
    public interface IRoleFactory
    {
        Task<PagedListWithRelatedResponse<RoleResponse>> BuildListAsync(IEnumerable<ApplicationRole> roles, CancellationToken cancellationToken = default);

        Task<EntityWithRelatedResponse<RoleResponse, RoleDetailsRelated>> BuildDetailsAsync(ApplicationRole role, CancellationToken cancellationToken = default);

        Task<PagedListWithRelatedResponse<RolePermissionResponse>> BuildPermissionsListAsync(IEnumerable<ApplicationRoleClaim> claims, CancellationToken cancellationToken = default);
    }
}
