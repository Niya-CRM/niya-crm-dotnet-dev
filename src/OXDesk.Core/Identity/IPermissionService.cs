using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Core.Identity;

public interface IPermissionService
{
    // Entity-centric operations (aligned with RoleService)
    Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    Task<Permission?> GetPermissionByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Permission> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default);
    Task<Permission> UpdatePermissionAsync(int id, UpdatePermissionRequest request, CancellationToken cancellationToken = default);
    Task<string[]> GetPermissionRolesAsync(int permissionId, CancellationToken cancellationToken = default);

    // Support for validation and potential maintenance operations
    Task<Permission?> GetPermissionByNameAsync(string normalizedName);
    Task<bool> DeletePermissionAsync(int id);
}
