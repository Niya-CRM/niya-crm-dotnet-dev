using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Core.Identity;

/// <summary>
/// Service interface for managing user signatures.
/// </summary>
public interface IUserSignatureService
{
    /// <summary>
    /// Gets the signature for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user signature if found; otherwise null.</returns>
    Task<UserSignature?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a user signature (upsert).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="request">The upsert request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created or updated user signature.</returns>
    Task<UserSignature> UpsertAsync(int userId, UpsertUserSignatureRequest request, CancellationToken cancellationToken = default);
}
