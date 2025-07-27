using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Core.Onboarding;
using NiyaCRM.Core.Onboarding.DTOs;
using NiyaCRM.Core.Tenants;

namespace NiyaCRM.Api.Controllers;

/// <summary>
/// MVC controller that hosts the initial setup wizard. Only used the very first time the
/// application starts. It creates the first tenant and an admin user.
/// </summary>
[Route("setup")]
public class SetupController : Controller
{
    private readonly IOnboardingService _onboardingService;
    private readonly ILogger<SetupController> _logger;

    public SetupController(IOnboardingService onboardingService, ILogger<SetupController> logger)
    {
        _onboardingService = onboardingService;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // If system already installed, redirect to login
        if (await _onboardingService.IsApplicationInstalledAsync(cancellationToken))
            return RedirectToAction("Login", "Auth");

        return View(new AppSetupDto());
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AppSetupDto model, CancellationToken cancellationToken)
    {
        if (await _onboardingService.IsApplicationInstalledAsync(cancellationToken))
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
            return View(model);
        }

        var installationDto = new AppInstallationDto
        {
            TenantName = model.TenantName,
            Host = model.Host,
            AdminFirstName = model.FirstName,
            AdminLastName = model.LastName,
            AdminEmail = model.AdminEmail,
            AdminPassword = model.Password,
            TimeZone = model.TimeZone
        };

        try
        {
            Tenant tenant = await _onboardingService.InstallApplicationAsync(installationDto, cancellationToken);
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

    [HttpGet("success")]
    public IActionResult Success()
    {
        return View();
    }
}
