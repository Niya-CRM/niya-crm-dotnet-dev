using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OXDesk.Core.Identity;

namespace OXDesk.Application.Identity;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Permission> AddPermissionAsync(Permission permission)
    {
        return await _permissionRepository.AddAsync(permission);
    }

    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        return await _permissionRepository.GetAllAsync();
    }

    public async Task<Permission?> GetPermissionByIdAsync(Guid id)
    {
        return await _permissionRepository.GetByIdAsync(id);
    }

    public async Task<Permission> UpdatePermissionAsync(Permission permission)
    {
        return await _permissionRepository.UpdateAsync(permission);
    }

    public async Task<bool> DeletePermissionAsync(Guid id)
    {
        return await _permissionRepository.DeleteAsync(id);
    }
}
