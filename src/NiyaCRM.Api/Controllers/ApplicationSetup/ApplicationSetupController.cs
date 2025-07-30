using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.ApplicationSetup;
using NiyaCRM.Core.ApplicationSetup.DTOs;
using NiyaCRM.Core.Tenants;

namespace NiyaCRM.Api.Controllers.ApplicationSetup;

/// <summary>
/// Controller for handling application setup and installation operations.
/// This controller provides both API endpoints and MVC views for the initial setup wizard.
/// </summary>
public class ApplicationSetupController : Controller
{
    private readonly ILogger<ApplicationSetupController> _logger;
    private readonly IApplicationSetupService _applicationSetupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSetupController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="applicationSetupService">The application setup service.</param>
    public ApplicationSetupController(ILogger<ApplicationSetupController> logger, IApplicationSetupService applicationSetupService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _applicationSetupService = applicationSetupService ?? throw new ArgumentNullException(nameof(applicationSetupService));
    }
    
    /// <summary>
    /// Displays the initial setup page if the application is not yet installed.
    /// Otherwise redirects to login.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The setup view or redirect to login.</returns>
    [HttpGet("setup")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // If system already installed, redirect to login
        if (await _applicationSetupService.IsApplicationInstalledAsync(cancellationToken))
            return RedirectToAction("Login", "Auth");

        return View(new AppInstallationDto());
    }

    /// <summary>
    /// Processes the setup form submission.
    /// </summary>
    /// <param name="model">The setup form data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Redirect to success page or show errors.</returns>
    [HttpPost("setup")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AppInstallationDto model, CancellationToken cancellationToken)
    {
        if (await _applicationSetupService.IsApplicationInstalledAsync(cancellationToken))
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
            return View(model);
        }
        
        // Use the model directly as it's already an AppInstallationDto
        var installationDto = model;

        try
        {
            Tenant tenant = await _applicationSetupService.InstallApplicationAsync(installationDto, cancellationToken);
            _logger.LogInformation("Application installed for tenant {TenantId}", tenant.Id);
            return RedirectToAction(nameof(Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Setup failed");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    /// <summary>
    /// Displays the success page after successful installation.
    /// </summary>
    /// <returns>The success view.</returns>
    [HttpGet("setup/success")]
    public IActionResult Success()
    {
        return View();
    }
}
