using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OXDesk.Core.Identity;

public interface IPermissionService
{
    Task<Permission> AddPermissionAsync(Permission permission);
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task<Permission?> GetPermissionByIdAsync(Guid id);
    Task<Permission?> GetPermissionByNameAsync(string normalizedName);
    Task<Permission> UpdatePermissionAsync(Permission permission);
    Task<bool> DeletePermissionAsync(Guid id);
}
