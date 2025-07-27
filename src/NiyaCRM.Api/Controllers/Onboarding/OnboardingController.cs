using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.Onboarding;
using NiyaCRM.Core.Onboarding.DTOs;
using NiyaCRM.Core.Tenants;

namespace NiyaCRM.Api.Controllers.Onboarding;

/// <summary>
/// Controller for handling application onboarding and installation operations.
/// </summary>
[ApiController]
[Route("api/onboarding")]
public class OnboardingController : ControllerBase
{
    private readonly ILogger<OnboardingController> _logger;
    private readonly IOnboardingService _onboardingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnboardingController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="onboardingService">The onboarding service.</param>
    public OnboardingController(ILogger<OnboardingController> logger, IOnboardingService onboardingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _onboardingService = onboardingService ?? throw new ArgumentNullException(nameof(onboardingService));
    }

    /// <summary>
    /// Installs the application and creates the first tenant and admin user.
    /// This endpoint should only be accessible during initial setup.
    /// </summary>
    /// <param name="installationDto">The installation details including tenant and admin information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the installation process.</returns>
    [HttpPost("install")]
    [ProducesResponseType(typeof(Tenant), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InstallApplication([FromBody] AppInstallationDto installationDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Starting application installation with tenant name: {TenantName}", installationDto.TenantName);
            
            // Check if the application is already installed
            bool isInstalled = await _onboardingService.IsApplicationInstalledAsync(cancellationToken);
            if (isInstalled)
            {
                _logger.LogWarning("Installation attempted but application is already installed");
                return Conflict(new ProblemDetails
                {
                    Title = CommonConstant.MESSAGE_CONFLICT,
                    Detail = "The application is already installed.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            // Process the installation
            var tenant = await _onboardingService.InstallApplicationAsync(installationDto, cancellationToken);
            
            _logger.LogInformation("Application installation completed successfully with tenant ID: {TenantId}", tenant.Id);
            
            // Return a 201 Created with the tenant information and a location header
            return CreatedAtAction(nameof(GetInstallationStatus), new { id = tenant.Id }, tenant);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid installation request: {Message}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = CommonConstant.MESSAGE_INVALID_REQUEST,
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Installation conflict: {Message}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = CommonConstant.MESSAGE_CONFLICT,
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during application installation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An unexpected error occurred during installation",
                Detail = "The installation process encountered an error. Please check the logs for more information.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Gets the status of an installation by tenant ID.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The installation status.</returns>
    [HttpGet("status/{id:guid}")]
    [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Tenant>> GetInstallationStatus(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting installation status for tenant ID: {TenantId}", id);
        
        // For now, we'll just check if the tenant exists
        // In a more complete implementation, we might check additional installation state
        var isInstalled = await _onboardingService.IsApplicationInstalledAsync(cancellationToken);
        if (!isInstalled)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Installation not found",
                Detail = "The application has not been installed yet.",
                Status = StatusCodes.Status404NotFound
            });
        }
        
        // Return just message as this endpoint is public
        return Ok(new { Message = $"Installation completed." });
    }
}
