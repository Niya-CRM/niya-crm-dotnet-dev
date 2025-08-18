using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Core.Identity
{
    public interface IRoleService
    {
        Task<IReadOnlyList<ApplicationRole>> GetAllRolesAsync(CancellationToken cancellationToken = default);
        Task<ApplicationRole?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApplicationRole> CreateRoleAsync(CreateRoleRequest request, Guid? createdBy = null, CancellationToken cancellationToken = default);
        Task<ApplicationRole> UpdateRoleAsync(Guid id, UpdateRoleRequest request, Guid? updatedBy = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteRoleAsync(Guid id, Guid? deletedBy = null, CancellationToken cancellationToken = default);

        Task<string[]> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<string[]> SetRolePermissionsAsync(Guid roleId, IEnumerable<string> permissionNames, Guid? updatedBy = null, CancellationToken cancellationToken = default);
    }
}
