using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OXDesk.Core.Identity;
using OXDesk.DbContext.Data;

namespace OXDesk.Identity.Data;

/// <summary>
/// Repository implementation for UserSignature entity operations.
/// </summary>
public class UserSignatureRepository : IUserSignatureRepository
{
    private readonly TenantDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSignatureRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The tenant database context.</param>
    public UserSignatureRepository(TenantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<UserSignature?> GetByUserIdAsync(int userId)
    {
        return await _dbContext.UserSignatures
            .Where(us => us.UserId == userId)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<UserSignature> AddAsync(UserSignature userSignature)
    {
        _dbContext.UserSignatures.Add(userSignature);
        await _dbContext.SaveChangesAsync();
        return userSignature;
    }

    /// <inheritdoc/>
    public async Task<UserSignature> UpdateAsync(UserSignature userSignature)
    {
        _dbContext.UserSignatures.Update(userSignature);
        await _dbContext.SaveChangesAsync();
        return userSignature;
    }
}
