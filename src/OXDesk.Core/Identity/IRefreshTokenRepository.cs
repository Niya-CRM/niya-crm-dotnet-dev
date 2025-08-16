using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OXDesk.Core.Identity
{
    /// <summary>
    /// Repository interface for managing RefreshToken entities.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Adds a new refresh token.
        /// </summary>
        Task<RefreshToken> AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all refresh tokens for a user.
        /// </summary>
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a refresh token by its hashed token value.
        /// </summary>
        Task<RefreshToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all refresh tokens for a user.
        /// Returns the number of tokens deleted.
        /// </summary>
        Task<int> DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a refresh token by its hashed token value.
        /// Returns true if a token was deleted.
        /// </summary>
        Task<bool> DeleteByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default);
    }
}
