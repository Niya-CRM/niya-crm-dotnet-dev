using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NiyaCRM.Core.Identity;

/// <summary>
/// Repository interface for Permission entity operations
/// </summary>
public interface IPermissionRepository
{
    Task<Permission> AddAsync(Permission permission);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<Permission?> GetByIdAsync(Guid id);
    Task<Permission?> GetByNameAsync(string normalizedName);
    Task<Permission> UpdateAsync(Permission permission);
    Task<bool> DeleteAsync(Guid id);
}
