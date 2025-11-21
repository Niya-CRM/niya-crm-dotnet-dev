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
        Task<ApplicationRole?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApplicationRole> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
        Task<ApplicationRole> UpdateRoleAsync(int id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteRoleAsync(int id, CancellationToken cancellationToken = default);

        Task<string[]> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
        Task<string[]> SetRolePermissionsAsync(int roleId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the underlying permission claims for the role including audit fields.
        /// </summary>
        Task<IReadOnlyList<ApplicationRoleClaim>> GetRolePermissionClaimsAsync(int roleId, CancellationToken cancellationToken = default);
    }
}
