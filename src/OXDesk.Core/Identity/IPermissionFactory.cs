using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Core.Identity
{
    /// <summary>
    /// Factory interface for building permission DTOs and response wrappers, including enrichment and related data.
    /// </summary>
    public interface IPermissionFactory
    {
        Task<PagedListWithRelatedResponse<PermissionResponse>> BuildListAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken = default);

        Task<EntityWithRelatedResponse<PermissionResponse, PermissionDetailsRelated>> BuildDetailsAsync(Permission permission, CancellationToken cancellationToken = default);
    }
}
