namespace OXDesk.Core.Settings;

/// <summary>
/// Repository interface for setting data access operations.
/// </summary>
public interface ISettingRepository
{
    /// <summary>
    /// Gets a setting by its unique key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The setting if found; otherwise null.</returns>
    Task<Setting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new setting record.
    /// </summary>
    /// <param name="entity">The setting to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added setting.</returns>
    Task<Setting> AddAsync(Setting entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing setting record.
    /// </summary>
    /// <param name="entity">The setting to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated setting.</returns>
    Task<Setting> UpdateAsync(Setting entity, CancellationToken cancellationToken = default);
}
