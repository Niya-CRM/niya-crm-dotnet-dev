namespace OXDesk.Core.Settings;

/// <summary>
/// Constants related to application settings.
/// </summary>
public static class SettingConstant
{
    /// <summary>
    /// Supported value type identifiers.
    /// </summary>
    public static class ValueTypes
    {
        /// <summary>Plain string value.</summary>
        public const string String = "string";

        /// <summary>Integer value.</summary>
        public const string Int = "int";

        /// <summary>JSON value.</summary>
        public const string Json = "json";

        /// <summary>Boolean value.</summary>
        public const string Bool = "bool";

        /// <summary>All valid value types.</summary>
        public static readonly string[] All =
        [
            String,
            Int,
            Json,
            Bool
        ];
    }

    /// <summary>
    /// Supported signature type identifiers.
    /// </summary>
    public static class SignatureTypes
    {
        /// <summary>Standard fixed-format signature.</summary>
        public const string StandardFixed = "standard-fixed";

        /// <summary>Free-style signature.</summary>
        public const string FreeStyle = "free-style";

        /// <summary>Global signature.</summary>
        public const string GlobalSignature = "global-signature";

        /// <summary>All valid signature types.</summary>
        public static readonly string[] All =
        [
            StandardFixed,
            FreeStyle,
            GlobalSignature
        ];
    }

    /// <summary>
    /// Well-known setting keys reusable across the application.
    /// </summary>
    public static class Keys
    {
        /// <summary>Signature setting key (stores JSON with signatureType and signatureContent).</summary>
        public const string Signature = "signature";

        /// <summary>All valid setting keys.</summary>
        public static readonly string[] All =
        [
            Signature
        ];
    }

    /// <summary>
    /// Maps each well-known key to its expected <see cref="ValueTypes"/> value.
    /// </summary>
    public static readonly Dictionary<string, string> KeyValueTypeMap = new()
    {
        { Keys.Signature, ValueTypes.Json }
    };

    /// <summary>
    /// Error messages for settings.
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>Error message when a setting is not found.</summary>
        public const string NotFound = "Setting not found.";

        /// <summary>Error message when the value type is invalid.</summary>
        public const string InvalidValueType = "Invalid value type.";

        /// <summary>Error message when the value cannot be parsed for the given type.</summary>
        public const string InvalidValueFormat = "Value does not match the specified value type.";

        /// <summary>Error message when the setting key is not recognised.</summary>
        public const string InvalidKey = "Invalid setting key.";

        /// <summary>Error message when the value type does not match the expected type for the key.</summary>
        public const string ValueTypeMismatch = "Value type does not match the expected type for this setting key.";

        /// <summary>Error message when the signature JSON is invalid.</summary>
        public const string InvalidSignatureJson = "Value must be valid JSON with 'signatureType' and 'signatureContent' properties.";

        /// <summary>Error message when the signature type is invalid.</summary>
        public const string InvalidSignatureType = "Invalid signature type.";

        /// <summary>Error message when signature content is required but missing.</summary>
        public const string SignatureContentRequired = "Signature content is required for global-signature type.";
    }
}
