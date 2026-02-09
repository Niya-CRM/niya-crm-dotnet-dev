namespace OXDesk.Core.Tickets;

/// <summary>
/// Repository interface for holiday data access operations.
/// </summary>
public interface IHolidayRepository
{
    /// <summary>
    /// Gets all holidays for a specific business hours schedule.
    /// </summary>
    /// <param name="businessHourId">The parent business hours identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of holidays.</returns>
    Task<IEnumerable<Holiday>> GetByBusinessHourIdAsync(int businessHourId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new holiday entry.
    /// </summary>
    /// <param name="holiday">The holiday entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added holiday entity.</returns>
    Task<Holiday> AddAsync(Holiday holiday, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a holiday entry.
    /// </summary>
    /// <param name="id">The holiday identifier.</param>
    /// <param name="deletedBy">The identifier of the user performing the deletion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the entity was soft deleted, otherwise false.</returns>
    Task<bool> SoftDeleteAsync(int id, int deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes all holidays for a specific business hours schedule.
    /// </summary>
    /// <param name="businessHourId">The parent business hours identifier.</param>
    /// <param name="deletedBy">The identifier of the user performing the deletion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if any entities were soft deleted, otherwise false.</returns>
    Task<bool> SoftDeleteByBusinessHourIdAsync(int businessHourId, int deletedBy, CancellationToken cancellationToken = default);
}
