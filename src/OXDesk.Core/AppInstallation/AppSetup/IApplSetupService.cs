using OXDesk.Core.AppInstallation.AppSetup.DTOs;
using OXDesk.Core.Tenants;

namespace OXDesk.Core.AppInstallation.AppSetup;

/// <summary>
/// Service interface for application setup and installation operations.
/// </summary>
public interface IAppSetupService
{
    /// <summary>
    /// Installs the application and sets up the first tenant and admin user.
    /// Also creates a system system user with inactive status.
    /// </summary>
    /// <param name="setupDto">The setup details including tenant and admin information.</param>
    /// <param name="tenantId">Optional tenant ID to use. If not provided, a new one will be generated.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tenant.</returns>
    Task<Tenant> InstallApplicationAsync(AppSetupDto setupDto, Guid? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the application has already been installed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the application is already installed, otherwise false.</returns>
    Task<bool> IsApplicationInstalledAsync(CancellationToken cancellationToken = default);
}
