using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.Common;
using OXDesk.Core.AppInstallation.AppSetup;
using OXDesk.Core.AppInstallation.AppSetup.DTOs;
using OXDesk.Core.Referentials;
using OXDesk.Core.Tenants;
using Microsoft.AspNetCore.Authorization;
using OXDesk.Core.ValueLists;
using OXDesk.Core.ValueLists.DTOs;

namespace OXDesk.Api.Controllers.AppSetup;

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
    private readonly IValueListService _valueListService;
    private readonly IValueListItemService _valueListItemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSetupController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="AppSetupService">The application setup service.</param>
    /// <param name="valueListService">The value list service.</param>
    /// <param name="valueListItemService">The value list item service.</param>
    public AppSetupController(ILogger<AppSetupController> logger, IAppSetupService AppSetupService, IValueListService valueListService, IValueListItemService valueListItemService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _AppSetupService = AppSetupService ?? throw new ArgumentNullException(nameof(AppSetupService));
        _valueListService = valueListService ?? throw new ArgumentNullException(nameof(valueListService));
        _valueListItemService = valueListItemService ?? throw new ArgumentNullException(nameof(valueListItemService));
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

        // Get active countries for dropdown from ValueList "Country"
        var countryList = await _valueListService.GetCountriesAsync(cancellationToken);
        var countries = new List<ValueListItemOption>();
        if (countryList != null)
        {
            countries = countryList
                .Where(i => i.IsActive)
                .OrderBy(i => i.ItemName)
                .ToList();
        }

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
        {
            await LoadCountriesAsync(cancellationToken);
            return View(model);
        }

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
            await LoadCountriesAsync(cancellationToken);
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
            await LoadCountriesAsync(cancellationToken);
            return View(model);
        }
    }

    private async Task LoadCountriesAsync(CancellationToken cancellationToken)
    {
        var countryList = await _valueListService.GetCountriesAsync(cancellationToken);

        ViewBag.Countries = countryList;
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
