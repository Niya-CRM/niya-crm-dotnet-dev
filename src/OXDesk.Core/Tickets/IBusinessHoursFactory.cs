using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Tickets.DTOs;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Factory interface for building business hours DTOs.
/// </summary>
public interface IBusinessHoursFactory
{
    /// <summary>
    /// Builds a list response from a collection of business hours entities.
    /// </summary>
    /// <param name="businessHours">The business hours entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list response with related data.</returns>
    Task<List<BusinessHoursResponse>> BuildListAsync(
        IEnumerable<BusinessHours> businessHours,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a detailed response for a single business hours entity with related custom hours and holidays.
    /// </summary>
    /// <param name="businessHours">The business hours entity.</param>
    /// <param name="customBusinessHours">The related custom business hours.</param>
    /// <param name="holidays">The related holidays.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An entity response with related data.</returns>
    Task<EntityWithRelatedResponse<BusinessHoursResponse, BusinessHoursDetailsRelated>> BuildDetailsAsync(
        BusinessHours businessHours,
        IEnumerable<CustomBusinessHours> customBusinessHours,
        IEnumerable<Holiday> holidays,
        CancellationToken cancellationToken = default);
}
