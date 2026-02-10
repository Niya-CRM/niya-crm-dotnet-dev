using System.Text.Json.Serialization;

namespace OXDesk.Core.Settings.DTOs;

/// <summary>
/// Request DTO for creating or updating a setting.
/// </summary>
public sealed class UpsertSettingRequest
{
    /// <summary>
    /// Gets or sets the setting value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value type.
    /// Must be one of the values defined in <see cref="SettingConstant.ValueTypes"/>.
    /// </summary>
    public string ValueType { get; set; } = SettingConstant.ValueTypes.String;
}

/// <summary>
/// Response DTO for a setting.
/// </summary>
public sealed class SettingResponse
{
    /// <summary>
    /// Gets or sets the setting identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the setting key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the setting value, cast to the appropriate type based on <see cref="ValueType"/>.
    /// For "int" the value is an integer, for "bool" a boolean, otherwise a string.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the value type identifier.
    /// </summary>
    public string ValueType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the user who created this setting.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this setting was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the display name of the user who created this setting.
    /// </summary>
    public string? CreatedByText { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this setting.
    /// </summary>
    public int UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this setting was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the display name of the user who last updated this setting.
    /// </summary>
    public string? UpdatedByText { get; set; }
}

/// <summary>
/// Strongly-typed value for the signature setting (stored as JSON).
/// </summary>
public sealed class SignatureSettingValue
{
    /// <summary>
    /// Gets or sets the signature type.
    /// Must be one of the values defined in <see cref="SettingConstant.SignatureTypes"/>.
    /// </summary>
    [JsonPropertyName("signatureType")]
    public string SignatureType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the signature content (HTML or other markup).
    /// Required when <see cref="SignatureType"/> is <see cref="SettingConstant.SignatureTypes.GlobalSignature"/>.
    /// </summary>
    [JsonPropertyName("signatureContent")]
    public string SignatureContent { get; set; } = string.Empty;
}
