using System.Globalization;
using System.Text;

namespace NiyaCRM.Core.Helpers.Naming
{
    /// <summary>
    /// Extension method to convert strings written in PascalCase or camelCase into snake_case.
    /// Put here so it can be reused by both Infrastructure and Core layers.
    /// </summary>
    public static class SnakeCaseHelper
    {
        /// <summary>
        /// Converts the provided <paramref name="name"/> to snake_case.
        /// Handles sequences of upper-case letters (e.g. "APIKey" -> "api_key").
        /// </summary>
        /// <param name="name">The string to convert.</param>
        /// <returns>snake_case representation.</returns>
        public static string ToSnakeCase(this string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var sb = new StringBuilder(name.Length + 4);
            UnicodeCategory? prevCategory = null;

            foreach (var c in name!)
            {
                var category = char.GetUnicodeCategory(c);

                if (category == UnicodeCategory.UppercaseLetter || category == UnicodeCategory.TitlecaseLetter)
                {
                    if (prevCategory is UnicodeCategory.LowercaseLetter or UnicodeCategory.DecimalDigitNumber)
                        sb.Append('_');

                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(char.ToLowerInvariant(c));
                }

                prevCategory = category;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts the provided <paramref name="name"/> into a lowercase hyphen-separated key (kebab-case).
        /// Replaces any sequence of non-alphanumeric characters (including spaces/underscores) with a single '-'.
        /// Examples: "Users" -> "users", "User Profiles" -> "user-profile".
        /// </summary>
        /// <param name="name">The input text.</param>
        /// <returns>kebab-case key.</returns>
        public static string ToKey(this string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var sb = new StringBuilder(name.Length);
            var lastWasHyphen = false;

            foreach (var ch in name!)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(char.ToLowerInvariant(ch));
                    lastWasHyphen = false;
                }
                else if (!lastWasHyphen)
                {
                    sb.Append('-');
                    lastWasHyphen = true;
                }
            }

            // Trim any leading/trailing hyphens
            return sb.ToString().Trim('-');
        }
    }
}
