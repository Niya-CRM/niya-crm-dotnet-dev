using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.Identity.DTOs;

namespace NiyaCRM.Infrastructure.Data.Identity;

/// <summary>
/// Repository implementation for user data access operations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    public UserRepository(
        ApplicationDbContext dbContext,
        ILogger<UserRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all users with pagination - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        // Query users with pagination directly from the database
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                CountryCode = u.CountryCode,
                TimeZone = u.TimeZone,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive == "Y",
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                CreatedBy = u.CreatedBy,
                UpdatedBy = u.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return users;
    }

    /// <inheritdoc />
    public async Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID: {UserId}", id);

        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                CountryCode = u.CountryCode,
                TimeZone = u.TimeZone,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive == "Y",
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                CreatedBy = u.CreatedBy,
                UpdatedBy = u.UpdatedBy
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<UserResponse>> GetAllUsersWithoutPaginationAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all users without pagination");

        // Query all users directly from the database without pagination
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                CountryCode = u.CountryCode,
                TimeZone = u.TimeZone,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive == "Y",
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                CreatedBy = u.CreatedBy,
                UpdatedBy = u.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}
