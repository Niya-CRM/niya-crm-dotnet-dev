using OXDesk.Core.Tickets.DTOs;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Service interface for business hours business operations.
/// </summary>
public interface IBusinessHoursService
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
    /// Creates a new business hours schedule.
    /// </summary>
    /// <param name="request">The creation request DTO.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created business hours entity.</returns>
    Task<BusinessHours> CreateAsync(CreateBusinessHoursRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches (partial update) an existing business hours schedule.
    /// If business_hours_type is changed to 24x7, linked CustomBusinessHours are deleted.
    /// </summary>
    /// <param name="id">The business hours identifier.</param>
    /// <param name="request">The patch request DTO.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated business hours entity, or null if not found.</returns>
    Task<BusinessHours?> UpdateAsync(int id, PatchBusinessHoursRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a business hours schedule and all linked custom business hours and holidays.
    /// </summary>
    /// <param name="id">The business hours identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the entity was soft deleted, otherwise false.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new holiday entry.
    /// </summary>
    /// <param name="request">The creation request DTO.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created holiday entity.</returns>
    Task<Holiday> CreateHolidayAsync(CreateHolidayRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a holiday entry belonging to a specific business hours schedule.
    /// </summary>
    /// <param name="businessHourId">The parent business hours identifier.</param>
    /// <param name="holidayId">The holiday identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the entity was soft deleted, otherwise false.</returns>
    Task<bool> DeleteHolidayAsync(int businessHourId, int holidayId, CancellationToken cancellationToken = default);
}
