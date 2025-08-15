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
    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        // Check if pagination should be applied
        bool usePagination = pageNumber.HasValue && pageSize.HasValue && pageNumber.Value > 0 && pageSize.Value > 0;
        
        if (usePagination)
        {
            _logger.LogDebug("Getting users with pagination - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        }
        else
        {
            _logger.LogDebug("Getting all users without pagination");
        }

        // Start building the query
        IQueryable<ApplicationUser> query = _dbContext.Users.AsNoTracking();
        
        // Always order by username
        IOrderedQueryable<ApplicationUser> orderedQuery = query.OrderBy(u => u.UserName);
            
        // Apply pagination if needed
        if (usePagination)
        {
            // Skip and Take don't change the IOrderedQueryable type
            query = orderedQuery
                .Skip((pageNumber!.Value - 1) * pageSize!.Value)
                .Take(pageSize.Value);
        }
        else
        {
            // Use the ordered query when not paginating
            query = orderedQuery;
        }

        // Execute query and map results
        var users = await query
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Location = u.Location,
                CountryCode = u.CountryCode,
                TimeZone = u.TimeZone,
                PhoneNumber = u.PhoneNumber,
                Profile = u.Profile,
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
                Location = u.Location,
                CountryCode = u.CountryCode,
                TimeZone = u.TimeZone,
                PhoneNumber = u.PhoneNumber,
                Profile = u.Profile,
                IsActive = u.IsActive == "Y",
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                CreatedBy = u.CreatedBy,
                UpdatedBy = u.UpdatedBy
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
    

}
