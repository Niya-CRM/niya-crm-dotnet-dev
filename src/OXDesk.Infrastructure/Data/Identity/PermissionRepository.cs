using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OXDesk.Core.Identity;
using OXDesk.Infrastructure.Data;

namespace OXDesk.Infrastructure.Data.Identity;

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
        return await _dbContext.Permissions.AsQueryable().ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(int id)
    {
        // Global query filter will ensure tenant isolation
        return await _dbContext.Permissions.Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Permission?> GetByNameAsync(string normalizedName)
    {
        return await _dbContext.Permissions
            .Where(p => p.NormalizedName == normalizedName)
            .FirstOrDefaultAsync();
    }

    public async Task<Permission> UpdateAsync(Permission permission)
    {
        _dbContext.Permissions.Update(permission);
        await _dbContext.SaveChangesAsync();
        return permission;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Global query filter will ensure tenant isolation
        var permission = await _dbContext.Permissions
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();
        if (permission == null) return false;
        
        _dbContext.Permissions.Remove(permission);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
