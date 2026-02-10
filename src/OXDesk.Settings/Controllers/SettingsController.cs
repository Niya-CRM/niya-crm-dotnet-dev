using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Common;
using OXDesk.Core.Settings;
using OXDesk.Core.Settings.DTOs;

namespace OXDesk.Settings.Controllers;

/// <summary>
/// Controller for managing application settings.
/// </summary>
[ApiController]
[Route("settings")]
public class SettingsController : ControllerBase
{
    private readonly ISettingService _service;
    private readonly ISettingFactory _factory;
    private readonly IValidator<UpsertSettingRequest> _validator;
    private readonly ILogger<SettingsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsController"/> class.
    /// </summary>
    /// <param name="service">The setting service.</param>
    /// <param name="factory">The setting factory.</param>
    /// <param name="validator">The upsert request validator.</param>
    /// <param name="logger">The logger.</param>
    public SettingsController(
        ISettingService service,
        ISettingFactory factory,
        IValidator<UpsertSettingRequest> validator,
        ILogger<SettingsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a setting by its key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The setting.</returns>
    /// <response code="200">Returns the setting.</response>
    /// <response code="404">Setting not found.</response>
    [HttpGet("{key}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(SettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SettingResponse>> GetByKey(string key, CancellationToken cancellationToken)
    {
        var entity = await _service.GetByKeyAsync(key, cancellationToken);
        if (entity == null)
            return NotFound(new { message = SettingConstant.ErrorMessages.NotFound });

        var result = await _factory.BuildResponseAsync(entity, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates or updates a setting identified by the given key.
    /// </summary>
    /// <param name="key">The setting key.</param>
    /// <param name="request">The upsert request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created or updated setting.</returns>
    /// <response code="200">Setting created or updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPatch("{key}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(SettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SettingResponse>> Upsert(
        string key,
        [FromBody] UpsertSettingRequest request,
        CancellationToken cancellationToken)
    {
        if (!SettingConstant.Keys.All.Contains(key))
            return BadRequest(new { message = SettingConstant.ErrorMessages.InvalidKey });

        var validationContext = new ValidationContext<UpsertSettingRequest>(request);
        validationContext.RootContextData[Validators.UpsertSettingRequestValidator.ContextKey] = key;
        var validation = await _validator.ValidateAsync(validationContext, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var entity = await _service.UpsertAsync(key, request, cancellationToken);
        var result = await _factory.BuildResponseAsync(entity, cancellationToken);
        return Ok(result);
    }
}
