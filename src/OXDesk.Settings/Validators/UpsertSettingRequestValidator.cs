using System.Text.Json;
using FluentValidation;
using OXDesk.Core.Settings;
using OXDesk.Core.Settings.DTOs;

namespace OXDesk.Settings.Validators;

/// <summary>
/// Validator for <see cref="UpsertSettingRequest"/>.
/// The setting key must be supplied via <c>RootContextData["Key"]</c> before validation.
/// </summary>
public class UpsertSettingRequestValidator : AbstractValidator<UpsertSettingRequest>
{
    /// <summary>
    /// Context data key used to pass the setting key from the controller.
    /// </summary>
    public const string ContextKey = "Key";

    /// <summary>
    /// Initializes a new instance of the <see cref="UpsertSettingRequestValidator"/> class.
    /// </summary>
    public UpsertSettingRequestValidator()
    {
        RuleFor(x => x.ValueType)
            .NotEmpty()
            .WithMessage("Value type is required.")
            .Must(type => SettingConstant.ValueTypes.All.Contains(type))
            .WithMessage(SettingConstant.ErrorMessages.InvalidValueType);

        RuleFor(x => x.ValueType)
            .Must((request, valueType, context) =>
            {
                var key = context.RootContextData.TryGetValue(ContextKey, out var k) ? k as string : null;
                if (key == null || !SettingConstant.KeyValueTypeMap.TryGetValue(key, out var expectedType))
                    return true;
                return valueType == expectedType;
            })
            .WithMessage(SettingConstant.ErrorMessages.ValueTypeMismatch);

        RuleFor(x => x)
            .Must(x => IsValueValidForType(x.Value, x.ValueType))
            .WithMessage(SettingConstant.ErrorMessages.InvalidValueFormat)
            .When(x => !string.IsNullOrEmpty(x.Value));

        RuleFor(x => x)
            .Must((request, _, context) =>
            {
                var key = context.RootContextData.TryGetValue(ContextKey, out var k) ? k as string : null;
                if (key != SettingConstant.Keys.Signature)
                    return true;
                return ValidateSignatureValue(request.Value);
            })
            .WithMessage(SettingConstant.ErrorMessages.InvalidSignatureJson)
            .When(x =>
            {
                return !string.IsNullOrEmpty(x.Value);
            });
    }

    /// <summary>
    /// Validates that the value can be parsed as the declared type.
    /// </summary>
    private static bool IsValueValidForType(string value, string valueType)
    {
        return valueType switch
        {
            SettingConstant.ValueTypes.Int => int.TryParse(value, out _),
            SettingConstant.ValueTypes.Bool => bool.TryParse(value, out _),
            SettingConstant.ValueTypes.Json => IsValidJson(value),
            _ => true
        };
    }

    /// <summary>
    /// Validates that the string is valid JSON.
    /// </summary>
    private static bool IsValidJson(string value)
    {
        try
        {
            JsonDocument.Parse(value);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates the signature-specific JSON structure:
    /// - Must be valid JSON with signatureType and signatureContent properties.
    /// - signatureType is always mandatory and must be a valid signature type.
    /// - signatureContent is mandatory when signatureType is global-signature.
    /// </summary>
    private static bool ValidateSignatureValue(string value)
    {
        try
        {
            var signature = JsonSerializer.Deserialize<SignatureSettingValue>(value);
            if (signature == null)
                return false;

            if (string.IsNullOrEmpty(signature.SignatureType))
                return false;

            if (!SettingConstant.SignatureTypes.All.Contains(signature.SignatureType))
                return false;

            if (signature.SignatureType == SettingConstant.SignatureTypes.GlobalSignature
                && string.IsNullOrEmpty(signature.SignatureContent))
                return false;

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
