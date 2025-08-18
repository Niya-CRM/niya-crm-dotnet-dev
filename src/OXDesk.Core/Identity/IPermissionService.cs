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
    Task<Permission?> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Permission> CreatePermissionAsync(CreatePermissionRequest request, Guid? createdBy = null, CancellationToken cancellationToken = default);
    Task<Permission> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request, Guid? updatedBy = null, CancellationToken cancellationToken = default);
    Task<string[]> GetPermissionRolesAsync(Guid permissionId, CancellationToken cancellationToken = default);

    // Support for validation and potential maintenance operations
    Task<Permission?> GetPermissionByNameAsync(string normalizedName);
    Task<bool> DeletePermissionAsync(Guid id);
}
