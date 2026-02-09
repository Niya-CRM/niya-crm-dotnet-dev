using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentValidation;
using OXDesk.Core.Common;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Tickets;
using OXDesk.Core.Tickets.DTOs;

namespace OXDesk.Tickets.Controllers;

/// <summary>
/// Controller for managing business hours, custom business hours, and holidays.
/// </summary>
[ApiController]
[Route("business-hours")]
public class BusinessHoursController : ControllerBase
{
    private readonly IBusinessHoursService _businessHoursService;
    private readonly IBusinessHoursFactory _businessHoursFactory;
    private readonly ICustomBusinessHoursRepository _customBusinessHoursRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IValidator<CreateBusinessHoursRequest> _createBusinessHoursValidator;
    private readonly IValidator<PatchBusinessHoursRequest> _patchBusinessHoursValidator;
    private readonly IValidator<CreateHolidayRequest> _createHolidayValidator;
    private readonly ILogger<BusinessHoursController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessHoursController"/> class.
    /// </summary>
    /// <param name="businessHoursService">The business hours service.</param>
    /// <param name="businessHoursFactory">The business hours factory.</param>
    /// <param name="customBusinessHoursRepository">The custom business hours repository.</param>
    /// <param name="holidayRepository">The holiday repository.</param>
    /// <param name="createBusinessHoursValidator">The create business hours request validator.</param>
    /// <param name="patchBusinessHoursValidator">The patch business hours request validator.</param>
    /// <param name="createHolidayValidator">The create holiday request validator.</param>
    /// <param name="logger">The logger.</param>
    public BusinessHoursController(
        IBusinessHoursService businessHoursService,
        IBusinessHoursFactory businessHoursFactory,
        ICustomBusinessHoursRepository customBusinessHoursRepository,
        IHolidayRepository holidayRepository,
        IValidator<CreateBusinessHoursRequest> createBusinessHoursValidator,
        IValidator<PatchBusinessHoursRequest> patchBusinessHoursValidator,
        IValidator<CreateHolidayRequest> createHolidayValidator,
        ILogger<BusinessHoursController> logger)
    {
        _businessHoursService = businessHoursService ?? throw new ArgumentNullException(nameof(businessHoursService));
        _businessHoursFactory = businessHoursFactory ?? throw new ArgumentNullException(nameof(businessHoursFactory));
        _customBusinessHoursRepository = customBusinessHoursRepository ?? throw new ArgumentNullException(nameof(customBusinessHoursRepository));
        _holidayRepository = holidayRepository ?? throw new ArgumentNullException(nameof(holidayRepository));
        _createBusinessHoursValidator = createBusinessHoursValidator ?? throw new ArgumentNullException(nameof(createBusinessHoursValidator));
        _patchBusinessHoursValidator = patchBusinessHoursValidator ?? throw new ArgumentNullException(nameof(patchBusinessHoursValidator));
        _createHolidayValidator = createHolidayValidator ?? throw new ArgumentNullException(nameof(createHolidayValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all business hours schedules.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of business hours.</returns>
    /// <response code="200">Returns the list of business hours.</response>
    [HttpGet]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(List<BusinessHoursResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BusinessHoursResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await _businessHoursService.GetAllAsync(cancellationToken);
        var result = await _businessHoursFactory.BuildListAsync(entities, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a single business hours schedule with related custom business hours and holidays.
    /// </summary>
    /// <param name="id">The business hours identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The business hours with related data.</returns>
    /// <response code="200">Returns the business hours with related data.</response>
    /// <response code="404">Business hours not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<BusinessHoursResponse, BusinessHoursDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<BusinessHoursResponse, BusinessHoursDetailsRelated>>> GetById(
        int id, CancellationToken cancellationToken)
    {
        var entity = await _businessHoursService.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return NotFound(new { message = BusinessHoursConstant.ErrorMessages.BusinessHoursNotFound });

        var customHours = await _customBusinessHoursRepository.GetByBusinessHourIdAsync(id, cancellationToken);
        var holidays = await _holidayRepository.GetByBusinessHourIdAsync(id, cancellationToken);

        var result = await _businessHoursFactory.BuildDetailsAsync(entity, customHours, holidays, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new business hours schedule with optional custom business hours.
    /// Custom hours are ignored when business_hours_type is "24x7".
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created business hours with related data.</returns>
    /// <response code="201">Business hours created successfully.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<BusinessHoursResponse, BusinessHoursDetailsRelated>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EntityWithRelatedResponse<BusinessHoursResponse, BusinessHoursDetailsRelated>>> Create(
        [FromBody] CreateBusinessHoursRequest request, CancellationToken cancellationToken)
    {
        var validation = await _createBusinessHoursValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var created = await _businessHoursService.CreateAsync(request, cancellationToken);

        var customHours = await _customBusinessHoursRepository.GetByBusinessHourIdAsync(created.Id, cancellationToken);
        var result = await _businessHoursFactory.BuildDetailsAsync(
            created, customHours, Enumerable.Empty<Holiday>(), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
    }

    /// <summary>
    /// Patches (partial update) an existing business hours schedule.
    /// If business_hours_type is "24x7", linked custom business hours are deleted.
    /// When custom_hours is provided, it replaces all existing custom hours.
    /// </summary>
    /// <param name="id">The business hours identifier.</param>
    /// <param name="request">The patch request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated business hours with related data.</returns>
    /// <response code="200">Business hours updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Business hours not found.</response>
    [HttpPatch("{id:int}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<BusinessHoursResponse, BusinessHoursDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<BusinessHoursResponse, BusinessHoursDetailsRelated>>> Patch(
        int id, [FromBody] PatchBusinessHoursRequest request, CancellationToken cancellationToken)
    {
        var validation = await _patchBusinessHoursValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var updated = await _businessHoursService.UpdateAsync(id, request, cancellationToken);
        if (updated == null)
            return NotFound(new { message = BusinessHoursConstant.ErrorMessages.BusinessHoursNotFound });

        var customHours = await _customBusinessHoursRepository.GetByBusinessHourIdAsync(id, cancellationToken);
        var holidays = await _holidayRepository.GetByBusinessHourIdAsync(id, cancellationToken);

        var result = await _businessHoursFactory.BuildDetailsAsync(updated, customHours, holidays, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a business hours schedule and all linked custom business hours and holidays.
    /// </summary>
    /// <param name="id">The business hours identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Business hours deleted successfully.</response>
    /// <response code="404">Business hours not found.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _businessHoursService.DeleteAsync(id, cancellationToken);
        if (!result)
            return NotFound(new { message = BusinessHoursConstant.ErrorMessages.BusinessHoursNotFound });

        return NoContent();
    }

    /// <summary>
    /// Creates a new holiday entry for a business hours schedule.
    /// </summary>
    /// <param name="id">The parent business hours identifier.</param>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created holiday.</returns>
    /// <response code="201">Holiday created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Parent business hours not found.</response>
    [HttpPost("{id:int}/holidays")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(HolidayResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HolidayResponse>> CreateHoliday(
        int id, [FromBody] CreateHolidayRequest request, CancellationToken cancellationToken)
    {
        request.BusinessHourId = id;

        var validation = await _createHolidayValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var parent = await _businessHoursService.GetByIdAsync(id, cancellationToken);
        if (parent == null)
            return NotFound(new { message = BusinessHoursConstant.ErrorMessages.BusinessHoursNotFound });

        var created = await _businessHoursService.CreateHolidayAsync(request, cancellationToken);

        var dto = new HolidayResponse
        {
            Id = created.Id,
            BusinessHourId = created.BusinessHourId,
            Name = created.Name,
            Date = created.Date,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt,
            CreatedBy = created.CreatedBy,
            UpdatedBy = created.UpdatedBy
        };

        return Created($"business-hours/{id}/holidays/{created.Id}", dto);
    }

    /// <summary>
    /// Soft deletes a holiday entry belonging to a specific business hours schedule.
    /// </summary>
    /// <param name="id">The parent business hours identifier.</param>
    /// <param name="holidayId">The holiday identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Holiday deleted successfully.</response>
    /// <response code="404">Holiday not found.</response>
    [HttpDelete("{id:int}/holidays/{holidayId:int}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHoliday(int id, int holidayId, CancellationToken cancellationToken)
    {
        var result = await _businessHoursService.DeleteHolidayAsync(id, holidayId, cancellationToken);
        if (!result)
            return NotFound(new { message = BusinessHoursConstant.ErrorMessages.HolidayNotFound });

        return NoContent();
    }
}
