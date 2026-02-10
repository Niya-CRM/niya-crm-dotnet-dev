using System.Text.Json;
using OXDesk.Core.Identity;
using OXDesk.Core.Settings;
using OXDesk.Core.Settings.DTOs;

namespace OXDesk.Settings.Services;

/// <summary>
/// Service implementation for managing application settings.
/// </summary>
public class SettingService : ISettingService
{
    private readonly ISettingRepository _repository;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingService"/> class.
    /// </summary>
    /// <param name="repository">The setting repository.</param>
    /// <param name="currentUser">The current user accessor.</param>
    public SettingService(
        ISettingRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    private int GetCurrentUserId()
    {
        return _currentUser.Id ?? throw new InvalidOperationException("Current user ID is null.");
    }

    /// <inheritdoc />
    public async Task<Setting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByKeyAsync(key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Setting> UpsertAsync(string key, UpsertSettingRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentUserId = GetCurrentUserId();

        var existing = await _repository.GetByKeyAsync(key, cancellationToken);

        if (existing != null)
        {
            existing.Value = request.Value;
            existing.ValueType = request.ValueType;
            existing.UpdatedAt = now;
            existing.UpdatedBy = currentUserId;

            return await _repository.UpdateAsync(existing, cancellationToken);
        }

        var entity = new Setting
        {
            Key = key,
            Value = request.Value,
            ValueType = request.ValueType,
            CreatedAt = now,
            CreatedBy = currentUserId,
            UpdatedAt = now,
            UpdatedBy = currentUserId
        };

        return await _repository.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SignatureSettingValue?> GetSignatureAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByKeyAsync(SettingConstant.Keys.Signature, cancellationToken);
        if (entity == null || string.IsNullOrEmpty(entity.Value))
            return null;

        return JsonSerializer.Deserialize<SignatureSettingValue>(entity.Value);
    }

    /// <inheritdoc />
    public object? CastValue(string value, string valueType)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return valueType switch
        {
            SettingConstant.ValueTypes.Int when int.TryParse(value, out var intVal) => intVal,
            SettingConstant.ValueTypes.Bool when bool.TryParse(value, out var boolVal) => boolVal,
            _ => value
        };
    }
}
