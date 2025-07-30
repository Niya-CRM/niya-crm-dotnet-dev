using NiyaCRM.Core.ApplicationSetup.DTOs;
using NiyaCRM.Core.Tenants;

namespace NiyaCRM.Core.ApplicationSetup;

/// <summary>
/// Service interface for application setup and installation operations.
/// </summary>
public interface IApplicationSetupService
{
    /// <summary>
    /// Installs the application and sets up the first tenant and admin user.
    /// Also creates a technical system user with inactive status.
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
