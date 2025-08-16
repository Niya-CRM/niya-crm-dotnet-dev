using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Referentials;

namespace OXDesk.Application.Referentials;

/// <summary>
/// Service implementation for reference data operations.
/// </summary>
public class ReferenceDataService : IReferenceDataService
{
    private readonly ICountryRepository _countryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceDataService"/> class.
    /// </summary>
    /// <param name="countryRepository">The country repository.</param>
    public ReferenceDataService(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    /// <summary>
    /// Gets all countries.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all countries.</returns>
    public async Task<IEnumerable<Country>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await _countryRepository.GetAllCountriesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a country by its two-letter country code (ISO 3166-1 alpha-2).
    /// </summary>
    /// <param name="countryCode">The two-letter country code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The country if found, otherwise null.</returns>
    public async Task<Country?> GetCountryByCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        return await _countryRepository.GetCountryByCodeAsync(countryCode, cancellationToken);
    }
}
