using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NiyaCRM.Core.Referentials;

/// <summary>
/// Service interface for reference data operations.
/// </summary>
public interface IReferenceDataService
{
    /// <summary>
    /// Gets all countries.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all countries.</returns>
    Task<IEnumerable<Country>> GetAllCountriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a country by its two-letter country code (ISO 3166-1 alpha-2).
    /// </summary>
    /// <param name="countryCode">The two-letter country code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The country if found, otherwise null.</returns>
    Task<Country?> GetCountryByCodeAsync(string countryCode, CancellationToken cancellationToken = default);
}
