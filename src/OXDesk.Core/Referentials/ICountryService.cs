using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OXDesk.Core.Referentials;

/// <summary>
/// Interface for country data service operations.
/// </summary>
public interface ICountryService
{
    /// <summary>
    /// Gets all active countries.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of active countries.</returns>
    Task<IEnumerable<Country>> GetActiveCountriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a country by its code.
    /// </summary>
    /// <param name="countryCode">The country code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The country if found; otherwise, null.</returns>
    Task<Country?> GetCountryByCodeAsync(string countryCode, CancellationToken cancellationToken = default);
}
