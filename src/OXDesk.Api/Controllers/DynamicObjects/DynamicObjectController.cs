using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.DynamicObjects.DTOs;
using OXDesk.Core.Common;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using OXDesk.Api.Common;

namespace OXDesk.Api.Controllers.DynamicObjects;

/// <summary>
/// Controller for managing dynamic object operations in the CRM system.
/// </summary>
[ApiController]
[Route("dynamic-objects")]
public class DynamicObjectController : ControllerBase
{
    private readonly IDynamicObjectService _dynamicObjectService;
    private readonly ILogger<DynamicObjectController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicObjectController"/> class.
    /// </summary>
    /// <param name="dynamicObjectService">The dynamic object service.</param>
    /// <param name="logger">The logger.</param>
    public DynamicObjectController(
        IDynamicObjectService dynamicObjectService,
        ILogger<DynamicObjectController> logger)
    {
        _dynamicObjectService = dynamicObjectService ?? throw new ArgumentNullException(nameof(dynamicObjectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // Helper methods moved to ControllerExtensions class

    /// <summary>
    /// Creates a new dynamic object.
    /// </summary>
    /// <param name="request">The dynamic object creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created dynamic object.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DynamicObject), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DynamicObject>> CreateDynamicObject([FromBody] DynamicObjectRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Creating dynamic object with name: {ObjectName}", request.ObjectName);
            
            var dynamicObject = await _dynamicObjectService.CreateDynamicObjectAsync(
                request: request,
                createdBy: CommonConstant.DEFAULT_TECHNICAL_USER,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully created dynamic object with ID: {DynamicObjectId}", dynamicObject.Id);
            return CreatedAtAction(nameof(GetDynamicObjectById), new { id = dynamicObject.Id }, dynamicObject);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in dynamic object creation request: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Dynamic object creation conflict: {Message}", ex.Message);
            return this.CreateConflictProblem(ex.Message);
        }
    }

    /// <summary>
    /// Gets a dynamic object by its identifier.
    /// </summary>
    /// <param name="objectId">The dynamic object identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dynamic object if found.</returns>
    [HttpGet("{objectId:guid}")]
    [ProducesResponseType(typeof(DynamicObject), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DynamicObject>> GetDynamicObjectById(Guid objectId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting dynamic object by ID: {DynamicObjectId}", objectId);
        
        var dynamicObject = await _dynamicObjectService.GetDynamicObjectByIdAsync(objectId, cancellationToken);
        if (dynamicObject == null)
        {
            _logger.LogWarning("Dynamic object not found: {DynamicObjectId}", objectId);
            return this.CreateNotFoundProblem($"Dynamic object with ID '{objectId}' not found.");
        }

        return Ok(dynamicObject);
    }

    /// <summary>
    /// Gets all dynamic objects with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1).</param>
    /// <param name="pageSize">The page size (default: 50, max: 100).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated collection of dynamic objects.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DynamicObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<DynamicObject>>> GetAllDynamicObjects(
        [FromQuery] int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        [FromQuery] int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all dynamic objects - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            
            var dynamicObjects = await _dynamicObjectService.GetAllDynamicObjectsAsync(pageNumber, pageSize, cancellationToken);
            return Ok(dynamicObjects);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid pagination parameters: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing dynamic object.
    /// </summary>
    /// <param name="objectId">The dynamic object identifier.</param>
    /// <param name="request">The dynamic object update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated dynamic object.</returns>
    [HttpPut("{objectId:guid}")]
    [ProducesResponseType(typeof(DynamicObject), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DynamicObject>> UpdateDynamicObject(Guid objectId, [FromBody] DynamicObjectRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var dynamicObject = await _dynamicObjectService.GetDynamicObjectByIdAsync(objectId, cancellationToken);
        if (dynamicObject == null)
        {
            _logger.LogWarning("Dynamic object not found: {DynamicObjectId}", objectId);
            return this.CreateNotFoundProblem($"Dynamic object with ID '{objectId}' not found.");
        }

        try
        {
            _logger.LogInformation("Updating dynamic object: {DynamicObjectId}", objectId);
            
            var updatedDynamicObject = await _dynamicObjectService.UpdateDynamicObjectAsync(
                objectId, 
                request, 
                CommonConstant.DEFAULT_TECHNICAL_USER, 
                cancellationToken);

            _logger.LogInformation("Successfully updated dynamic object: {DynamicObjectId}", objectId);
            return Ok(updatedDynamicObject);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid dynamic object update request: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Dynamic object not found for update: {DynamicObjectId}", objectId);
            return NotFound(new ProblemDetails
            {
                Title = "Dynamic Object Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Dynamic object update conflict: {Message}", ex.Message);
            return this.CreateConflictProblem(ex.Message);
        }
    }
}
