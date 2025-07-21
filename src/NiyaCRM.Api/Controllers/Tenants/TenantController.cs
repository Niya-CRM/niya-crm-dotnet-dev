using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Core.Tenants.DTOs;
using NiyaCRM.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Api.Controllers.Tenants;

/// <summary>
/// Controller for managing tenant operations in the multi-tenant CRM system.
/// </summary>
[Route("api/tenants")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantController"/> class.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="logger">The logger.</param>
    public TenantController(ITenantService tenantService, ILogger<TenantController> logger)
    {
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="request">The tenant creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tenant.</returns>
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
        try
        {
            _logger.LogInformation("Creating tenant with name: {Name}", request.Name);
            
            var tenant = await _tenantService.CreateTenantAsync(
                name: request.Name,
                host: request.Host,
                email: request.Email,
                databaseName: request.DatabaseName,
                createdBy: request.CreatedBy,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully created tenant with ID: {TenantId}", tenant.Id);
            return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, tenant);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid tenant creation request: {Message}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = CommonConstant.MESSAGE_INVALID_REQUEST,
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant creation conflict: {Message}", ex.Message);
            return Conflict(new ProblemDetails
            {
                Title = CommonConstant.MESSAGE_CONFLICT,
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
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
        try
        {
            _logger.LogInformation("Updating tenant: {TenantId}", id);
            
            var tenant = await _tenantService.UpdateTenantAsync(
                id: id,
                name: request.Name,
                host: request.Host,
                email: request.Email,
                databaseName: request.DatabaseName,
                modifiedBy: request.ModifiedBy,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully updated tenant: {TenantId}", id);
            return Ok(tenant);
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
    public async Task<ActionResult<Tenant>> ActivateTenant(Guid id, [FromBody] ActivateTenantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Activating tenant: {TenantId}", id);
            
            var tenant = await _tenantService.ActivateTenantAsync(id, request.ModifiedBy, cancellationToken);
            
            _logger.LogInformation("Successfully activated tenant: {TenantId}", id);
            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant not found for activation: {TenantId}", id);
            return NotFound(new ProblemDetails
            {
                Title = TenantConstant.MESSAGE_TENANT_NOT_FOUND,
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
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
    public async Task<ActionResult<Tenant>> DeactivateTenant(Guid id, [FromBody] DeactivateTenantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deactivating tenant: {TenantId}", id);
            
            var tenant = await _tenantService.DeactivateTenantAsync(id, request.ModifiedBy, cancellationToken);
            
            _logger.LogInformation("Successfully deactivated tenant: {TenantId}", id);
            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant not found for deactivation: {TenantId}", id);
            return NotFound(new ProblemDetails
            {
                Title = TenantConstant.MESSAGE_TENANT_NOT_FOUND,
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}
