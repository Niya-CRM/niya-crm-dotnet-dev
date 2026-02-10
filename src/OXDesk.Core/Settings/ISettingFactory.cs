using OXDesk.Core.Settings.DTOs;

namespace OXDesk.Core.Settings;

/// <summary>
/// Factory interface for building setting response DTOs.
/// </summary>
public interface ISettingFactory
{
    /// <summary>
    /// Builds a response DTO from a setting entity.
    /// Casts the value to the appropriate CLR type based on <see cref="Setting.ValueType"/>.
    /// </summary>
    /// <param name="entity">The setting entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The setting response DTO.</returns>
    Task<SettingResponse> BuildResponseAsync(Setting entity, CancellationToken cancellationToken = default);
}
