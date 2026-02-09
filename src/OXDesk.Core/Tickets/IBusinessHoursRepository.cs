namespace OXDesk.Core.Tickets;

/// <summary>
/// Repository interface for business hours data access operations.
/// </summary>
public interface IBusinessHoursRepository
{
    /// <summary>
    /// Gets all business hours schedules.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of business hours.</returns>
    Task<IEnumerable<BusinessHours>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a business hours schedule by its identifier.
    /// </summary>
    /// <param name="id">The business hours identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The business hours if found, otherwise null.</returns>
    Task<BusinessHours?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new business hours schedule.
    /// </summary>
    /// <param name="businessHours">The business hours entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added business hours entity.</returns>
    Task<BusinessHours> AddAsync(BusinessHours businessHours, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing business hours schedule.
    /// </summary>
    /// <param name="businessHours">The business hours entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated business hours entity.</returns>
    Task<BusinessHours> UpdateAsync(BusinessHours businessHours, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a business hours schedule.
    /// </summary>
    /// <param name="id">The business hours identifier.</param>
    /// <param name="deletedBy">The identifier of the user performing the deletion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the entity was soft deleted, otherwise false.</returns>
    Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the IsDefault flag on all business hours except the specified one.
    /// </summary>
    /// <param name="excludeId">The business hours identifier to exclude from clearing.</param>
    /// <param name="updatedBy">The identifier of the user performing the update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ClearDefaultsAsync(int excludeId, int updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether any non-deleted business hours exist.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if any non-deleted business hours exist.</returns>
    Task<bool> AnyExistAsync(CancellationToken cancellationToken = default);
}
