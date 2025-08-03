using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NiyaCRM.Core.Identity;

public interface IPermissionService
{
    Task<Permission> AddPermissionAsync(Permission permission);
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task<Permission?> GetPermissionByIdAsync(Guid id);
    Task<Permission> UpdatePermissionAsync(Permission permission);
    Task<bool> DeletePermissionAsync(Guid id);
}
