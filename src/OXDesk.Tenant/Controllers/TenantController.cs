using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Tenants;
using OXDesk.Core.Tenants.DTOs;
using OXDesk.Core.Common;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using OXDesk.Tenant.Validators;
using OXDesk.Tenant.Factories;
using OXDesk.Core.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OXDesk.Tenant.Controllers;

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
    private readonly ITenantFactory _tenantFactory;
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantController"/> class.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="activateDeactivateTenantRequestValidator">The validator for activate/deactivate tenant requests.</param>
    /// <param name="createTenantRequestValidator">The validator for create tenant requests.</param>
    /// <param name="tenantFactory">The tenant factory for building response DTOs.</param>
    /// <param name="currentTenant">The current tenant context.</param>
    public TenantController(
        ITenantService tenantService, 
        ILogger<TenantController> logger, 
        IValidator<ActivateDeactivateTenantRequest> activateDeactivateTenantRequestValidator,
        IValidator<CreateTenantRequest> createTenantRequestValidator,
        ITenantFactory tenantFactory,
        ICurrentTenant currentTenant)
    {
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activateDeactivateTenantRequestValidator = activateDeactivateTenantRequestValidator ?? throw new ArgumentNullException(nameof(activateDeactivateTenantRequestValidator));
        _createTenantRequestValidator = createTenantRequestValidator ?? throw new ArgumentNullException(nameof(createTenantRequestValidator));
        _tenantFactory = tenantFactory ?? throw new ArgumentNullException(nameof(tenantFactory));
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
    }

    /// <summary>
    /// Gets the current tenant based on the request context.
    /// Returns only public information without personal data.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current tenant public information if found.</returns>
    [HttpGet("current")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TenantPublicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantPublicResponse>> GetCurrentTenant(CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenant.Id;
        
        if (tenantId == null)
        {
            _logger.LogWarning("No current tenant context available");
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "No current tenant context available.", Status = StatusCodes.Status404NotFound });
        }

        _logger.LogDebug("Getting current tenant: {TenantId}", tenantId);
        
        var tenant = await _tenantService.GetTenantByIdAsync(tenantId.Value, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Current tenant not found: {TenantId}", tenantId);
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Tenant with ID '{tenantId}' was not found.", Status = StatusCodes.Status404NotFound });
        }
        
        var response = _tenantFactory.BuildPublicResponse(tenant);
        return Ok(response);
    }

    // <summary>
    // Creates a new tenant.
    // </summary>
    // <param name="request">The tenant creation request.</param>
    // <param name="cancellationToken">The cancellation token.</param>
    // <returns>The created tenant.</returns>
    [HttpPost]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>>> CreateTenant([FromBody] CreateTenantRequest request, CancellationToken cancellationToken = default)
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
            var response = await _tenantFactory.BuildDetailsAsync(tenant, cancellationToken);
            return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, response);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in tenant creation request: {Message}", ex.Message);
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant creation conflict: {Message}", ex.Message);
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = ex.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    /// <summary>
    /// Gets a tenant by its identifier.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found.</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>>> GetTenantById(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by ID: {TenantId}", id);
        
        var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found: {TenantId}", id);
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Tenant with ID '{id}' was not found.", Status = StatusCodes.Status404NotFound });
        }
        var response = await _tenantFactory.BuildDetailsAsync(tenant, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Gets a tenant by its host.
    /// </summary>
    /// <param name="host">The tenant host.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found.</returns>
    [HttpGet("host/{host}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>>> GetTenantByHost(string host, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tenant by host: {Host}", host);
        
        var tenant = await _tenantService.GetTenantByHostAsync(host, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found for host: {Host}", host);
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Tenant with host '{host}' was not found.", Status = StatusCodes.Status404NotFound });
        }
        var response = await _tenantFactory.BuildDetailsAsync(tenant, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Gets all active tenants.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active tenants.</returns>
    [HttpGet("active")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(PagedListWithRelatedResponse<TenantResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedListWithRelatedResponse<TenantResponse>>> GetActiveTenants(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active tenants");
        
        var tenants = await _tenantService.GetActiveTenantsAsync(cancellationToken);
        var response = await _tenantFactory.BuildListAsync(tenants, 1, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Gets all tenants with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1).</param>
    /// <param name="pageSize">The page size (default: 50, max: 100).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated collection of tenants.</returns>
    [HttpGet]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(PagedListWithRelatedResponse<TenantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedListWithRelatedResponse<TenantResponse>>> GetAllTenants(
        [FromQuery] int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        [FromQuery] int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all tenants - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            
            var tenants = await _tenantService.GetAllTenantsAsync(pageNumber, pageSize, cancellationToken);
            var response = await _tenantFactory.BuildListAsync(tenants, pageNumber, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid pagination parameters: {Message}", ex.Message);
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
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
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>>> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant not found: {TenantId}", id);
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Tenant with ID '{id}' was not found.", Status = StatusCodes.Status404NotFound });
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
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully updated tenant: {TenantId}", id);
            var response = await _tenantFactory.BuildDetailsAsync(updatedTenant, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid tenant update request: {Message}", ex.Message);
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Tenant not found for update: {TenantId}", id);
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message, Status = StatusCodes.Status404NotFound });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant update conflict: {Message}", ex.Message);
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = ex.Message, Status = StatusCodes.Status409Conflict });
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
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>>> ActivateTenant(Guid id, [FromBody] ActivateDeactivateTenantRequest request, CancellationToken cancellationToken = default)
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
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>>> DeactivateTenant(Guid id, [FromBody] ActivateDeactivateTenantRequest request, CancellationToken cancellationToken = default)
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
    private async Task<ActionResult<EntityWithRelatedResponse<TenantResponse, TenantDetailsRelated>>> ChangeTenantActivationStatus(Guid id, ActivateDeactivateTenantRequest request, bool activate, CancellationToken cancellationToken = default)
    {
        var validationResult = await _activateDeactivateTenantRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)), Status = StatusCodes.Status400BadRequest });
        }

        try
        {
            string action = activate ? TenantConstant.ActivationAction.Activate : TenantConstant.ActivationAction.Deactivate;
            string actionVerb = activate ? "Activating" : "Deactivating";
            _logger.LogInformation("{ActionVerb} tenant: {TenantId}", actionVerb, id);
            
            var tenant = await _tenantService.ChangeTenantActivationStatusAsync(id, action, request.Reason, cancellationToken);
            
            string completedAction = activate ? "activated" : "deactivated";
            _logger.LogInformation("Successfully {CompletedAction} tenant: {TenantId}", completedAction, id);
            var response = await _tenantFactory.BuildDetailsAsync(tenant, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            string action = activate ? "activation" : "deactivation";
            _logger.LogWarning(ex, "Tenant not found for {Action}: {TenantId}", action, id);
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }
}
