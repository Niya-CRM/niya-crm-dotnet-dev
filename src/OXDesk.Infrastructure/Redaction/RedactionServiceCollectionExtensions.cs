using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using OXDesk.Core.Common.Redaction;

namespace OXDesk.Infrastructure.Redaction
{
    /// <summary>
    /// Service collection extensions for configuring data redaction.
    /// </summary>
    public static class RedactionServiceCollectionExtensions
    {
        /// <summary>
        /// Registers and configures redactors for masking sensitive data such as email and password.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>IServiceCollection for chaining.</returns>
        public static IServiceCollection AddOxDeskRedaction(this IServiceCollection services)
        {
            services.AddRedaction(redactionBuilder =>
            {
                redactionBuilder.SetRedactor<ErasingRedactor>(
                    RedactionTaxonomy.SensitiveData,
                    RedactionTaxonomy.PersonalData
                );
                redactionBuilder.SetFallbackRedactor<ErasingRedactor>();
            });

            return services;
        }
    }
}
