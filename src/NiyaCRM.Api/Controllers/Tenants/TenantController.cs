using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Core.Tenants.DTOs;
using NiyaCRM.Core.Common;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace NiyaCRM.Api.Controllers.Tenants;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantController"/> class.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="logger">The logger.</param>
    public TenantController(ITenantService tenantService, ILogger<TenantController> logger, IValidator<ActivateDeactivateTenantRequest> activateDeactivateTenantRequestValidator)
    {
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activateDeactivateTenantRequestValidator = activateDeactivateTenantRequestValidator ?? throw new ArgumentNullException(nameof(activateDeactivateTenantRequestValidator));
    }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="request">The tenant creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tenant.</returns>
    // [HttpPost]
    // [ProducesResponseType(typeof(Tenant), StatusCodes.Status201Created)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    // public async Task<ActionResult<Tenant>> CreateTenant([FromBody] CreateTenantRequest request, CancellationToken cancellationToken = default)
    // {
    //     if (!ModelState.IsValid)
    //     {
    //         return BadRequest(ModelState);
    //     }
    //     try
    //     {
    //         _logger.LogInformation("Creating tenant with name: {Name}", request.Name);
            
    //         var tenant = await _tenantService.CreateTenantAsync(
    //             name: request.Name,
    //             host: request.Host,
    //             email: request.Email,
    //             databaseName: request.DatabaseName,
    //             cancellationToken: cancellationToken);

    //         _logger.LogInformation("Successfully created tenant with ID: {TenantId}", tenant.Id);
    //         return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, tenant);
    //     }
    //     catch (ArgumentException ex)
    //     {
    //         _logger.LogWarning(ex, "Invalid tenant creation request: {Message}", ex.Message);
    //         return BadRequest(new ProblemDetails
    //         {
    //             Title = CommonConstant.MESSAGE_INVALID_REQUEST,
    //             Detail = ex.Message,
    //             Status = StatusCodes.Status400BadRequest
    //         });
    //     }
    //     catch (InvalidOperationException ex)
    //     {
    //         _logger.LogWarning(ex, "Tenant creation conflict: {Message}", ex.Message);
    //         return Conflict(new ProblemDetails
    //         {
    //             Title = CommonConstant.MESSAGE_CONFLICT,
    //             Detail = ex.Message,
    //             Status = StatusCodes.Status409Conflict
    //         });
    //     }
    // }

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
            return NotFound(new ProblemDetails
            {
                Title = TenantConstant.MESSAGE_TENANT_NOT_FOUND,
                Detail = $"Tenant with ID '{id}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
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
            return NotFound(new ProblemDetails
            {
                Title = TenantConstant.MESSAGE_TENANT_NOT_FOUND,
                Detail = $"Tenant with host '{host}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
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
            return BadRequest(new ProblemDetails
            {
                Title = CommonConstant.MESSAGE_INVALID_REQUEST,
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
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
            return NotFound(new ProblemDetails
            {
                Title = TenantConstant.MESSAGE_TENANT_NOT_FOUND,
                Detail = $"Tenant with ID '{id}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        try
        {
            _logger.LogInformation("Updating tenant: {TenantId}", id);
            
            var updatedTenant = await _tenantService.UpdateTenantAsync(
                id: id,
                name: request.Name ?? tenant.Name,
                host: request.Host ?? tenant.Host,
                email: request.Email ?? tenant.Email,
                userId: tenant.UserId,
                timeZone: request.TimeZone ?? tenant.TimeZone,
                databaseName: request.DatabaseName ?? tenant.DatabaseName,
                modifiedBy: CommonConstant.DEFAULT_USER,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully updated tenant: {TenantId}", id);
            return Ok(updatedTenant);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid tenant update request: {Message}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = CommonConstant.MESSAGE_INVALID_REQUEST,
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Tenant not found for update: {TenantId}", id);
            return NotFound(new ProblemDetails
            {
                Title = TenantConstant.MESSAGE_TENANT_NOT_FOUND,
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant update conflict: {Message}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = CommonConstant.MESSAGE_CONFLICT,
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
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
            return BadRequest(new ProblemDetails
            {
                Title = CommonConstant.MESSAGE_INVALID_REQUEST,
                Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            string action = activate ? "Activating" : "Deactivating";
            _logger.LogInformation("{Action} tenant: {TenantId}", action, id);
            
            Tenant tenant;
            if (activate)
            {
                tenant = await _tenantService.ActivateTenantAsync(id, request.Reason, cancellationToken);
            }
            else
            {
                tenant = await _tenantService.DeactivateTenantAsync(id, request.Reason, cancellationToken);
            }
            
            string completedAction = activate ? "activated" : "deactivated";
            _logger.LogInformation("Successfully {CompletedAction} tenant: {TenantId}", completedAction, id);
            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            string action = activate ? "activation" : "deactivation";
            _logger.LogWarning(ex, "Tenant not found for {Action}: {TenantId}", action, id);
            return NotFound(new ProblemDetails
            {
                Title = TenantConstant.MESSAGE_TENANT_NOT_FOUND,
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}
