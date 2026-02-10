using OXDesk.Core.Settings.DTOs;

namespace OXDesk.Core.Settings;

/// <summary>
/// Service interface for managing application settings.
/// </summary>
public interface ISettingService
{
    /// <summary>
    /// Gets a setting by its unique key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The setting if found; otherwise null.</returns>
    Task<Setting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a setting identified by the given key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="request">The upsert request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created or updated setting.</returns>
    Task<Setting> UpsertAsync(string key, UpsertSettingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the signature setting as a strongly-typed <see cref="SignatureSettingValue"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The signature setting value if found; otherwise null.</returns>
    Task<SignatureSettingValue?> GetSignatureAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Casts the raw string value of a setting to the appropriate CLR type based on its <see cref="Setting.ValueType"/>.
    /// </summary>
    /// <param name="value">The raw string value.</param>
    /// <param name="valueType">The value type identifier.</param>
    /// <returns>The cast value (int, bool, or string).</returns>
    object? CastValue(string value, string valueType);
}
