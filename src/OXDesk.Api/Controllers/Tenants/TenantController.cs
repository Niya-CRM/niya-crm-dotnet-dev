using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.Tenants;
using OXDesk.Core.Tenants.DTOs;
using OXDesk.Core.Common;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using OXDesk.Api.Validators.Tenants;
using OXDesk.Api.Common;

namespace OXDesk.Api.Controllers.Tenants;

/// <summary>
/// Controller for managing tenant operations in the multi-tenant CRM system.
/// </summary>
[ApiController]
[Route("tenants")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantController> _logger;
    private readonly IValidator<ActivateDeactivateTenantRequest> _activateDeactivateTenantRequestValidator;
    private readonly IValidator<CreateTenantRequest> _createTenantRequestValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantController"/> class.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="activateDeactivateTenantRequestValidator">The validator for activate/deactivate tenant requests.</param>
    /// <param name="createTenantRequestValidator">The validator for create tenant requests.</param>
    public TenantController(
        ITenantService tenantService, 
        ILogger<TenantController> logger, 
        IValidator<ActivateDeactivateTenantRequest> activateDeactivateTenantRequestValidator,
        IValidator<CreateTenantRequest> createTenantRequestValidator)
    {
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activateDeactivateTenantRequestValidator = activateDeactivateTenantRequestValidator ?? throw new ArgumentNullException(nameof(activateDeactivateTenantRequestValidator));
        _createTenantRequestValidator = createTenantRequestValidator ?? throw new ArgumentNullException(nameof(createTenantRequestValidator));
    }

    // <summary>
    // Creates a new tenant.
    // </summary>
    // <param name="request">The tenant creation request.</param>
    // <param name="cancellationToken">The cancellation token.</param>
    // <returns>The created tenant.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Tenant), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Tenant>> CreateTenant([FromBody] CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Use FluentValidation to validate the request
        var validationResult = await _createTenantRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Creating tenant with name: {Name}", request.Name);
            
            var tenant = await _tenantService.CreateTenantAsync(
                request: request,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully created tenant with ID: {TenantId}", tenant.Id);
            return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, tenant);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in tenant creation request: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant creation conflict: {Message}", ex.Message);
            return this.CreateConflictProblem(ex.Message);
        }
    }

    /// <summary>
    /// Gets a tenant by its identifier.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Tenant>> GetTenantById(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by ID: {TenantId}", id);
        
        var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found: {TenantId}", id);
            return this.CreateNotFoundProblem($"Tenant with ID '{id}' was not found.");
        }

        return Ok(tenant);
    }

    /// <summary>
    /// Gets a tenant by its host.
    /// </summary>
    /// <param name="host">The tenant host.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found.</returns>
    [HttpGet("host/{host}")]
    [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Tenant>> GetTenantByHost(string host, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by host: {Host}", host);
        
        var tenant = await _tenantService.GetTenantByHostAsync(host, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for host: {Host}", host);
            return this.CreateNotFoundProblem($"Tenant with host '{host}' was not found.");
        }

        return Ok(tenant);
    }

    /// <summary>
    /// Gets all active tenants.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active tenants.</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<Tenant>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Tenant>>> GetActiveTenants(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active tenants");
        
        var tenants = await _tenantService.GetActiveTenantsAsync(cancellationToken);
        return Ok(tenants);
    }

    /// <summary>
    /// Gets all tenants with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1).</param>
    /// <param name="pageSize">The page size (default: 50, max: 100).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated collection of tenants.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Tenant>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<Tenant>>> GetAllTenants(
        [FromQuery] int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        [FromQuery] int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all tenants - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            
            var tenants = await _tenantService.GetAllTenantsAsync(pageNumber, pageSize, cancellationToken);
            return Ok(tenants);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid pagination parameters: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="request">The tenant update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated tenant.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Tenant>> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found: {TenantId}", id);
            return this.CreateNotFoundProblem($"Tenant with ID '{id}' was not found.");
        }

        try
        {
            _logger.LogInformation("Updating tenant: {TenantId}", id);
            
            var updateRequest = new UpdateTenantRequest
            {
                Name = request.Name ?? tenant.Name,
                Host = request.Host ?? tenant.Host,
                Email = request.Email ?? tenant.Email,
                UserId = tenant.UserId,
                TimeZone = request.TimeZone ?? tenant.TimeZone,
                DatabaseName = request.DatabaseName ?? tenant.DatabaseName
            };
            
            var updatedTenant = await _tenantService.UpdateTenantAsync(
                id, 
                updateRequest, 
                CommonConstant.DEFAULT_SYSTEM_USER, 
                cancellationToken);

            _logger.LogInformation("Successfully updated tenant: {TenantId}", id);
            return Ok(updatedTenant);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid tenant update request: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Tenant not found for update: {TenantId}", id);
            return this.CreateNotFoundProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant update conflict: {Message}", ex.Message);
            return this.CreateConflictProblem(ex.Message);
        }
    }

    /// <summary>
    /// Activates a tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="request">The activation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activated tenant.</returns>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Tenant>> ActivateTenant(Guid id, [FromBody] ActivateDeactivateTenantRequest request, CancellationToken cancellationToken = default)
    {
        return await ChangeTenantActivationStatus(id, request, true, cancellationToken);
    }

    /// <summary>
    /// Deactivates a tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="request">The deactivation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deactivated tenant.</returns>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(Tenant), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Tenant>> DeactivateTenant(Guid id, [FromBody] ActivateDeactivateTenantRequest request, CancellationToken cancellationToken = default)
    {
        return await ChangeTenantActivationStatus(id, request, false, cancellationToken);
    }

    /// <summary>
    /// Private helper method to handle tenant activation status changes.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="request">The activation/deactivation request.</param>
    /// <param name="activate">True to activate, false to deactivate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>ActionResult with the updated tenant.</returns>
    private async Task<ActionResult<Tenant>> ChangeTenantActivationStatus(Guid id, ActivateDeactivateTenantRequest request, bool activate, CancellationToken cancellationToken = default)
    {
        var validationResult = await _activateDeactivateTenantRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.CreateBadRequestProblem(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        try
        {
            string action = activate ? TenantConstant.ActivationAction.Activate : TenantConstant.ActivationAction.Deactivate;
            string actionVerb = activate ? "Activating" : "Deactivating";
            _logger.LogInformation("{ActionVerb} tenant: {TenantId}", actionVerb, id);
            
            var tenant = await _tenantService.ChangeTenantActivationStatusAsync(id, action, request.Reason, cancellationToken);
            
            string completedAction = activate ? "activated" : "deactivated";
            _logger.LogInformation("Successfully {CompletedAction} tenant: {TenantId}", completedAction, id);
            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            string action = activate ? "activation" : "deactivation";
            _logger.LogWarning(ex, "Tenant not found for {Action}: {TenantId}", action, id);
            return this.CreateNotFoundProblem(ex.Message);
        }
    }
}
