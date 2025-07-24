using NiyaCRM.Core.Onboarding.DTOs;
using NiyaCRM.Core.Tenants;

namespace NiyaCRM.Core.Onboarding;

/// <summary>
/// Service interface for application onboarding and installation operations.
/// </summary>
public interface IOnboardingService
{
    /// <summary>
    /// Installs the application and sets up the first tenant and admin user.
    /// </summary>
    /// <param name="installationDto">The installation details including tenant and admin information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tenant.</returns>
    Task<Tenant> InstallApplicationAsync(AppInstallationDto installationDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the application has already been installed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the application is already installed, otherwise false.</returns>
    Task<bool> IsApplicationInstalledAsync(CancellationToken cancellationToken = default);
}
