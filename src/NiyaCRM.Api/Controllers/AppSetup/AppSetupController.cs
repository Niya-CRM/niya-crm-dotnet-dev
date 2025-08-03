using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.AppInstallation.AppSetup;
using NiyaCRM.Core.AppInstallation.AppSetup.DTOs;
using NiyaCRM.Core.Referentials;
using NiyaCRM.Core.Tenants;
using Microsoft.AspNetCore.Authorization;

namespace NiyaCRM.Api.Controllers.AppSetup;

/// <summary>
/// Controller for handling application setup and installation operations.
/// This controller provides both API endpoints and MVC views for the initial setup wizard.
/// </summary>
[Route("setup")]
[AllowAnonymous]
public class AppSetupController : Controller
{
    private readonly ILogger<AppSetupController> _logger;
    private readonly IAppSetupService _AppSetupService;
    private readonly IReferenceDataService _referenceDataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSetupController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="AppSetupService">The application setup service.</param>
    /// <param name="referenceDataService">The reference data service.</param>
    public AppSetupController(ILogger<AppSetupController> logger, IAppSetupService AppSetupService, IReferenceDataService referenceDataService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _AppSetupService = AppSetupService ?? throw new ArgumentNullException(nameof(AppSetupService));
        _referenceDataService = referenceDataService ?? throw new ArgumentNullException(nameof(referenceDataService));
    }
    
    /// <summary>
    /// Displays the initial setup page if the application is not yet installed.
    /// Otherwise redirects to login.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The setup view or redirect to login.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // If system already installed, redirect to login
        if (await _AppSetupService.IsApplicationInstalledAsync(cancellationToken))
            return RedirectToAction("Login", "Auth");

        // Get active countries for dropdown
        var countries = (await _referenceDataService.GetAllCountriesAsync(cancellationToken))
            .Where(c => c.IsActive == "Y")
            .OrderBy(c => c.CountryName)
            .ToList();

        // Pass countries to ViewBag for dropdown
        ViewBag.Countries = countries;

        return View(new AppSetupDto());
    }

    /// <summary>
    /// Processes the setup form submission.
    /// </summary>
    /// <param name="model">The setup form data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Redirect to success page or show errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AppSetupDto model, CancellationToken cancellationToken)
    {
        if (await _AppSetupService.IsApplicationInstalledAsync(cancellationToken))
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
            return View(model);
        }
        
        // Use the model directly as it's already an AppSetupDto
        var setupDto = model;

        try
        {
            Tenant tenant = await _AppSetupService.InstallApplicationAsync(setupDto, cancellationToken);
            _logger.LogInformation("Application installed for tenant {TenantId}", tenant.Id);
            return RedirectToAction(nameof(Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application installation failed");
            ModelState.AddModelError(string.Empty, CommonConstant.MESSAGE_INTERNAL_SERVER_ERROR);
            return View(model);
        }
    }

    /// <summary>
    /// Displays the success page after successful installation.
    /// </summary>
    /// <returns>The success view.</returns>
    [HttpGet("success")]
    public IActionResult Success()
    {
        return View();
    }
}
