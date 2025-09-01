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

    public async Task<IEnumerable<Permission>> GetAllAsync(Guid tenantId)
    {
        var query = _dbContext.Permissions.AsQueryable()
            .Where(p => p.TenantId == tenantId);
        
        return await query.ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(Guid id, Guid tenantId)
    {
        // FindAsync doesn't support filtering by additional conditions, so we use FirstOrDefaultAsync
        var query = _dbContext.Permissions.Where(p => p.Id == id)
            .Where(p => p.TenantId == tenantId);
        
        return await query.FirstOrDefaultAsync();
    }

    public async Task<Permission?> GetByNameAsync(string normalizedName, Guid tenantId)
    {
        var query = _dbContext.Permissions.Where(p => p.NormalizedName == normalizedName)
            .Where(p => p.TenantId == tenantId);
        
        return await query.FirstOrDefaultAsync();
    }

    public async Task<Permission> UpdateAsync(Permission permission)
    {
        _dbContext.Permissions.Update(permission);
        await _dbContext.SaveChangesAsync();
        return permission;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        // Find the permission with tenant filter
        var query = _dbContext.Permissions.Where(p => p.Id == id)
            .Where(p => p.TenantId == tenantId);
        
        var permission = await query.FirstOrDefaultAsync();
        if (permission == null) return false;
        
        _dbContext.Permissions.Remove(permission);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
