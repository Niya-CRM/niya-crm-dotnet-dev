using NiyaCRM.Core.Identity.DTOs;

namespace NiyaCRM.Core.Identity;

/// <summary>
/// Repository interface for user data access operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets all users with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated collection of user responses.</returns>
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a user by their identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user response if found, otherwise null.</returns>
    Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all users without pagination.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>All user responses.</returns>
    Task<IEnumerable<UserResponse>> GetAllUsersWithoutPaginationAsync(CancellationToken cancellationToken = default);
}
