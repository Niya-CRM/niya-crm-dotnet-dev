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
    /// Repository implementation for managing RefreshToken entities.
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RefreshTokenRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<RefreshToken> AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(token);
            // Ensure timestamps are set
            var now = DateTime.UtcNow;
            if (token.CreatedAt == default)
            {
                token.CreatedAt = now;
            }
            token.UpdatedAt = now;
            await _dbContext.RefreshTokens.AddAsync(token, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return token;
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                .AsNoTracking()
                .Where(rt => rt.UserId == userId)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<RefreshToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(hashedToken))
                throw new ArgumentException("Hashed token cannot be null or empty.", nameof(hashedToken));

            return await _dbContext.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.HashedToken == hashedToken, cancellationToken);
        }

        public async Task<int> DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync(cancellationToken);

            if (tokens.Count == 0) return 0;

            _dbContext.RefreshTokens.RemoveRange(tokens);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> DeleteByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(hashedToken))
                throw new ArgumentException("Hashed token cannot be null or empty.", nameof(hashedToken));

            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.HashedToken == hashedToken, cancellationToken);

            if (token == null) return false;

            _dbContext.RefreshTokens.Remove(token);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
