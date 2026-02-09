namespace OXDesk.Core.Tickets;

/// <summary>
/// Repository interface for custom business hours data access operations.
/// </summary>
public interface ICustomBusinessHoursRepository
{
    /// <summary>
    /// Gets all custom business hours for a specific business hours schedule.
    /// </summary>
    /// <param name="businessHourId">The parent business hours identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of custom business hours.</returns>
    Task<IEnumerable<CustomBusinessHours>> GetByBusinessHourIdAsync(int businessHourId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new custom business hours entry.
    /// </summary>
    /// <param name="customBusinessHours">The custom business hours entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added custom business hours entity.</returns>
    Task<CustomBusinessHours> AddAsync(CustomBusinessHours customBusinessHours, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple custom business hours entries.
    /// </summary>
    /// <param name="items">The custom business hours entities to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddRangeAsync(IEnumerable<CustomBusinessHours> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes all custom business hours entries for a specific business hours schedule.
    /// </summary>
    /// <param name="businessHourId">The parent business hours identifier.</param>
    /// <param name="deletedBy">The identifier of the user performing the deletion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if any entities were soft deleted, otherwise false.</returns>
    Task<bool> SoftDeleteByBusinessHourIdAsync(int businessHourId, int deletedBy, CancellationToken cancellationToken = default);
}
