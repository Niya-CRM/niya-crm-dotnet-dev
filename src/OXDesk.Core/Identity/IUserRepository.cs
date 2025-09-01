using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Core.Identity;

/// <summary>
/// Repository interface for user data access operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets all users with optional pagination.
    /// </summary>
    /// <param name="pageNumber">The page number. If null or less than 1, returns all users without pagination.</param>
    /// <param name="pageSize">The page size. If null or less than 1, returns all users without pagination.</param>
    /// <param name="tenantId">The tenant ID to filter users by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of user responses, paginated if pageNumber and pageSize are provided.</returns>
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(Guid tenantId, int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a user by their identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="tenantId">The tenant ID to filter users by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user response if found, otherwise null.</returns>
    Task<UserResponse?> GetUserByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
