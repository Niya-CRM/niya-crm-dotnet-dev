using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OXDesk.Core.Identity;

/// <summary>
/// Repository interface for Permission entity operations
/// </summary>
public interface IPermissionRepository
{
    Task<Permission> AddAsync(Permission permission);
    Task<IEnumerable<Permission>> GetAllAsync(Guid tenantId);
    Task<Permission?> GetByIdAsync(Guid id, Guid tenantId);
    Task<Permission?> GetByNameAsync(string normalizedName, Guid tenantId);
    Task<Permission> UpdateAsync(Permission permission);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}
