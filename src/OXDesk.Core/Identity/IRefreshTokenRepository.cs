using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OXDesk.Core.Identity
{
    /// <summary>
    /// Repository interface for managing UserRefreshToken entities.
    /// </summary>
    public interface IUserRefreshTokenRepository
    {
        /// <summary>
        /// Adds a new refresh token.
        /// </summary>
        Task<UserRefreshToken> AddAsync(UserRefreshToken token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all refresh tokens for a user.
        /// </summary>
        Task<IEnumerable<UserRefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a refresh token by its hashed token value.
        /// </summary>
        Task<UserRefreshToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing refresh token entity.
        /// </summary>
        Task<UserRefreshToken> UpdateAsync(UserRefreshToken token, CancellationToken cancellationToken = default);

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
