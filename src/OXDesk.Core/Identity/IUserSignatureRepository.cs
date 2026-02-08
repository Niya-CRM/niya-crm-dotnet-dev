using System.Threading.Tasks;

namespace OXDesk.Core.Identity;

/// <summary>
/// Repository interface for UserSignature entity operations.
/// </summary>
public interface IUserSignatureRepository
{
    /// <summary>
    /// Gets a user signature by user ID.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The user signature if found; otherwise null.</returns>
    Task<UserSignature?> GetByUserIdAsync(int userId);

    /// <summary>
    /// Adds a new user signature.
    /// </summary>
    /// <param name="userSignature">The user signature to add.</param>
    /// <returns>The added user signature.</returns>
    Task<UserSignature> AddAsync(UserSignature userSignature);

    /// <summary>
    /// Updates an existing user signature.
    /// </summary>
    /// <param name="userSignature">The user signature to update.</param>
    /// <returns>The updated user signature.</returns>
    Task<UserSignature> UpdateAsync(UserSignature userSignature);
}
