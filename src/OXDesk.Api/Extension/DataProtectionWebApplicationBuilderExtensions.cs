using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;

namespace OXDesk.Api.Extension
{
    /// <summary>
    /// Service registration helpers for configuring Data Protection and cookie naming.
    /// </summary>
    public static class DataProtectionWebApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures ASP.NET Core Data Protection application name and returns a stable cookie prefix.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        /// <returns>The cookie prefix to be used for application cookies.</returns>
        public static string ConfigureDataProtectionAndGetCookiePrefix(this WebApplicationBuilder builder)
        {
            var configuredApplicationName = builder.Configuration["ApplicationName"];
            var applicationName = string.IsNullOrWhiteSpace(configuredApplicationName)
                ? builder.Environment.ApplicationName
                : configuredApplicationName;

            var cookiePrefixName = Regex.Replace(applicationName ?? string.Empty, "[^A-Za-z0-9]", string.Empty);
            if (string.IsNullOrWhiteSpace(cookiePrefixName))
            {
                cookiePrefixName = "App";
            }

            var cookiePrefix = $".{cookiePrefixName}";

            builder.Services
                .AddDataProtection()
                .SetApplicationName(applicationName ?? cookiePrefixName);

            return cookiePrefix;
        }
    }
}
