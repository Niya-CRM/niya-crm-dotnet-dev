using OXDesk.Core.Common;
using OXDesk.Core.Identity.DTOs;
using System.Collections.Generic;

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
    Task<UserDetailsResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Changes the activation status of a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="action">The action to perform (activate or deactivate).</param>
    /// <param name="reason">The reason for the status change.</param>
    /// <param name="changedBy">The user performing the change.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user response.</returns>
    Task<UserResponse> ChangeUserActivationStatusAsync(Guid id, string action, string reason, Guid? changedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a dictionary of users keyed by their Ids for the provided set of userIds.
    /// This is useful for batch lookups (e.g., CreatedBy/UpdatedBy enrichment) to avoid N+1 queries.
    /// </summary>
    /// <param name="userIds">Collection of user IDs to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only dictionary mapping userId to ApplicationUser.</returns>
    Task<IReadOnlyDictionary<Guid, ApplicationUser>> GetUsersLookupByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single user's display name by Id with caching support.
    /// </summary>
    Task<string?> GetUserNameByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
