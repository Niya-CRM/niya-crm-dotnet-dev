using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NiyaCRM.Core.Referentials;
using NiyaCRM.Infrastructure.Data;

namespace NiyaCRM.Infrastructure.Data.ReferenceData;

/// <summary>
/// Repository implementation for country reference data access operations.
/// </summary>
public class CountryRepository : ICountryRepository
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public CountryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets all countries.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of all countries.</returns>
    public async Task<IEnumerable<Country>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Country>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a country by its two-letter country code (ISO 3166-1 alpha-2).
    /// </summary>
    /// <param name="countryCode">The two-letter country code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The country if found, otherwise null.</returns>
    public async Task<Country?> GetCountryByCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Country>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CountryCode == countryCode, cancellationToken);
    }
}
