using OXDesk.Core.Identity;
using OXDesk.Core.Settings;
using OXDesk.Core.Settings.DTOs;

namespace OXDesk.Settings.Factories;

/// <summary>
/// Builds Setting response DTOs from entities.
/// </summary>
public sealed class SettingFactory : ISettingFactory
{
    private readonly IUserService _userService;
    private readonly ISettingService _settingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingFactory"/> class.
    /// </summary>
    /// <param name="userService">The user service for resolving display names.</param>
    /// <param name="settingService">The setting service for value casting.</param>
    public SettingFactory(IUserService userService, ISettingService settingService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
    }

    /// <inheritdoc />
    public async Task<SettingResponse> BuildResponseAsync(Setting entity, CancellationToken cancellationToken = default)
    {
        var dto = new SettingResponse
        {
            Id = entity.Id,
            Key = entity.Key,
            Value = _settingService.CastValue(entity.Value, entity.ValueType),
            ValueType = entity.ValueType,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedBy = entity.UpdatedBy,
            UpdatedAt = entity.UpdatedAt
        };

        dto.CreatedByText = await _userService.GetUserNameByIdAsync(dto.CreatedBy, cancellationToken);
        dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);

        return dto;
    }
}
