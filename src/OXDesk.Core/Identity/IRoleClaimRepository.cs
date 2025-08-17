using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OXDesk.Core.Identity;

/// <summary>
/// Repository interface for managing role claims (permissions) for roles.
/// </summary>
public interface IRoleClaimRepository
{
    /// <summary>
    /// Removes role claims for the given role with the specified claim type and values.
    /// </summary>
    Task RemoveRoleClaimsAsync(Guid roleId, string claimType, IEnumerable<string> claimValues, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a batch of role claims.
    /// </summary>
    Task AddRoleClaimsAsync(IEnumerable<ApplicationRoleClaim> claims, CancellationToken cancellationToken = default);
}
