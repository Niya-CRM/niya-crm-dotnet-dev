using Microsoft.EntityFrameworkCore;
using OXDesk.Core.Identity;
using OXDesk.DbContext.Data;

namespace OXDesk.Identity.Data;

/// <summary>
/// Repository implementation for managing UserRefreshToken entities.
/// </summary>
public class UserRefreshTokenRepository : IUserRefreshTokenRepository
{
    private readonly TenantDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRefreshTokenRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The tenant database context.</param>
    public UserRefreshTokenRepository(TenantDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<IEnumerable<UserRefreshToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<UserRefreshToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hashedToken))
            throw new ArgumentException("Hashed token cannot be null or empty.", nameof(hashedToken));

        return await _dbContext.UserRefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.HashedToken == hashedToken, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<UserRefreshToken> UpdateAsync(UserRefreshToken token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);
        token.UpdatedAt = DateTime.UtcNow;
        _dbContext.UserRefreshTokens.Update(token);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return token;
    }

    /// <inheritdoc/>
    public async Task<int> DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbContext.UserRefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);

        if (tokens.Count == 0) return 0;

        _dbContext.UserRefreshTokens.RemoveRange(tokens);
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
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
