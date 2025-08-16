using OXDesk.Core.Common;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Core.Identity;

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
    /// Gets all users.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all users.</returns>
    Task<UsersListResponse> GetAllUsersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current user's unique identifier from claims.
    /// </summary>
    /// <returns>The current user's Guid.</returns>
    /// <exception cref="InvalidOperationException">Thrown if user id claim is not found.</exception>
    Guid GetCurrentUserId();
}
