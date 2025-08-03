using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NiyaCRM.Core.Identity;
using NiyaCRM.Infrastructure.Data;

namespace NiyaCRM.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Permission entity operations
/// </summary>
public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PermissionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Permission> AddAsync(Permission permission)
    {
        _dbContext.Permissions.Add(permission);
        await _dbContext.SaveChangesAsync();
        return permission;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _dbContext.Permissions.ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Permissions.FindAsync(id);
    }

    public async Task<Permission?> GetByNameAsync(string normalizedName)
    {
        return await _dbContext.Permissions
            .FirstOrDefaultAsync(p => p.NormalizedName == normalizedName);
    }

    public async Task<Permission> UpdateAsync(Permission permission)
    {
        _dbContext.Permissions.Update(permission);
        await _dbContext.SaveChangesAsync();
        return permission;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var permission = await _dbContext.Permissions.FindAsync(id);
        if (permission == null) return false;
        
        _dbContext.Permissions.Remove(permission);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
