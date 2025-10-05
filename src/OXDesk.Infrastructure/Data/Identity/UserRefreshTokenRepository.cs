using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OXDesk.Core.Identity;

namespace OXDesk.Infrastructure.Data.Identity
{
    /// <summary>
    /// Repository implementation for managing UserRefreshToken entities.
    /// </summary>
    public class UserRefreshTokenRepository : IUserRefreshTokenRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRefreshTokenRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<UserRefreshToken> AddAsync(UserRefreshToken token, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(token);
            // Ensure timestamps are set
            var now = DateTime.UtcNow;
            if (token.CreatedAt == default)
            {
                token.CreatedAt = now;
            }
            token.UpdatedAt = now;
            await _dbContext.UserRefreshTokens.AddAsync(token, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return token;
        }

        public async Task<IEnumerable<UserRefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserRefreshTokens
                .AsNoTracking()
                .Where(rt => rt.UserId == userId)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserRefreshToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(hashedToken))
                throw new ArgumentException("Hashed token cannot be null or empty.", nameof(hashedToken));

            return await _dbContext.UserRefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.HashedToken == hashedToken, cancellationToken);
        }

        public async Task<UserRefreshToken> UpdateAsync(UserRefreshToken token, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(token);
            token.UpdatedAt = DateTime.UtcNow;
            _dbContext.UserRefreshTokens.Update(token);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return token;
        }

        public async Task<int> DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _dbContext.UserRefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync(cancellationToken);

            if (tokens.Count == 0) return 0;

            _dbContext.UserRefreshTokens.RemoveRange(tokens);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> DeleteByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(hashedToken))
                throw new ArgumentException("Hashed token cannot be null or empty.", nameof(hashedToken));

            var token = await _dbContext.UserRefreshTokens
                .FirstOrDefaultAsync(rt => rt.HashedToken == hashedToken, cancellationToken);

            if (token == null) return false;

            _dbContext.UserRefreshTokens.Remove(token);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
