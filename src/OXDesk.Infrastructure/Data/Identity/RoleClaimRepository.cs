using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OXDesk.Core.Identity;
using OXDesk.Infrastructure.Data;

namespace OXDesk.Infrastructure.Data.Identity;

/// <summary>
/// Repository implementation for managing role claims (permissions).
/// </summary>
public class RoleClaimRepository : IRoleClaimRepository
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleClaimRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public RoleClaimRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task RemoveRoleClaimsAsync(Guid roleId, string claimType, IEnumerable<string> claimValues, CancellationToken cancellationToken = default)
    {
        var values = claimValues?.ToArray() ?? Array.Empty<string>();
        if (values.Length == 0) return;

        var toRemove = await _dbContext.RoleClaims
            .Where(rc => rc.RoleId == roleId && rc.ClaimType == claimType && values.Contains(rc.ClaimValue))
            .ToListAsync(cancellationToken);

        if (toRemove.Count == 0) return;

        _dbContext.RoleClaims.RemoveRange(toRemove);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddRoleClaimsAsync(IEnumerable<ApplicationRoleClaim> claims, CancellationToken cancellationToken = default)
    {
        var list = claims?.ToList() ?? new List<ApplicationRoleClaim>();
        if (list.Count == 0) return;

        await _dbContext.RoleClaims.AddRangeAsync(list, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ApplicationRoleClaim>> GetRoleClaimsAsync(Guid roleId, string claimType, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.RoleClaims
            .Where(rc => rc.RoleId == roleId && rc.ClaimType == claimType)
            .OrderBy(rc => rc.ClaimValue);
        var list = await query.ToListAsync(cancellationToken);
        return list;
    }
}
