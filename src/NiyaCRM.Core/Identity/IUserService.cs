using NiyaCRM.Core.Common;
using NiyaCRM.Core.Identity.DTOs;

namespace NiyaCRM.Core.Identity;

/// <summary>
/// Service interface for user management operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <param name="createdBy">The user creating the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user response.</returns>
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, Guid? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found, otherwise null.</returns>
    Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their identifier with display values.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user with display values if found, otherwise null.</returns>
    Task<UserResponseWithDisplay?> GetUserByIdWithDisplayAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated collection of users.</returns>
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all users with display values and pagination.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated collection of user responses with display values.</returns>
    Task<IEnumerable<UserResponseWithDisplay>> GetAllUsersWithDisplayAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Caches all users individually with display values.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task CacheAllUsersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a user with display values from cache by ID.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user response with display values if found in cache, otherwise null.</returns>
    Task<UserResponseWithDisplay?> GetUserFromCacheAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a user's full name (first name and last name concatenated) from cache by ID.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's full name if found in cache, otherwise null.</returns>
    Task<string> GetUserFullNameFromCacheAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the current user's unique identifier from claims.
    /// </summary>
    /// <returns>The current user's Guid.</returns>
    /// <exception cref="InvalidOperationException">Thrown if user id claim is not found.</exception>
    Guid GetCurrentUserId();
}
