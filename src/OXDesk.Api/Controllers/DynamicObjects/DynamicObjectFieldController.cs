using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Api.Common;
using OXDesk.Core.Common;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.DynamicObjects.Fields;
using OXDesk.Core.DynamicObjects.Fields.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;

namespace OXDesk.Api.Controllers.DynamicObjects;

/// <summary>
/// Controller for managing dynamic object field definitions and field types.
/// </summary>
[ApiController]
[Route("dynamic-objects")]
public class DynamicObjectFieldController : ControllerBase
{
    private readonly IDynamicObjectFieldService _fieldService;
    private readonly ILogger<DynamicObjectFieldController> _logger;
    private readonly IDynamicObjectFieldFactory _fieldFactory;

    public DynamicObjectFieldController(
        IDynamicObjectFieldService fieldService,
        ILogger<DynamicObjectFieldController> logger,
        IDynamicObjectFieldFactory fieldFactory)
    {
        _fieldService = fieldService ?? throw new ArgumentNullException(nameof(fieldService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fieldFactory = fieldFactory ?? throw new ArgumentNullException(nameof(fieldFactory));
    }

    // Field Types

    /// <summary>
    /// Gets all available field types.
    /// </summary>
    [HttpGet("field-types")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(PagedListWithRelatedResponse<DynamicObjectFieldType>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedListWithRelatedResponse<DynamicObjectFieldType>>> GetAllFieldTypes(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all DynamicObject field types");
        var types = await _fieldService.GetAllFieldTypesAsync(cancellationToken);
        return Ok(new PagedListWithRelatedResponse<DynamicObjectFieldType>
        {
            Data = types,
            PageNumber = 1,
            RowCount = types.Count(),
            Related = Array.Empty<object>()
        });
    }

    /// <summary>
    /// Gets a field type by its identifier.
    /// </summary>
    [HttpGet("field-types/{fieldTypeId:int}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<DynamicObjectFieldType, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<DynamicObjectFieldType, object>>> GetFieldTypeById(int fieldTypeId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting field type by ID: {FieldTypeId}", fieldTypeId);
        var type = await _fieldService.GetFieldTypeByIdAsync(fieldTypeId, cancellationToken);
        if (type == null)
        {
            return this.CreateNotFoundProblem($"Field type with ID '{fieldTypeId}' not found.");
        }
        return Ok(new EntityWithRelatedResponse<DynamicObjectFieldType, object>
        {
            Data = type,
            Related = new object()
        });
    }

    // Fields

    /// <summary>
    /// Lists all fields for a given dynamic object ID.
    /// </summary>
    [HttpGet("{objectId:int}/fields")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(PagedListWithRelatedResponse<DynamicObjectFieldResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedListWithRelatedResponse<DynamicObjectFieldResponse>>> GetFieldsByObjectId(int objectId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting fields for object ID: {ObjectId}", objectId);
            var fields = await _fieldService.GetFieldsByObjectIdAsync(objectId, cancellationToken);
            var response = await _fieldFactory.BuildListAsync(fields, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid object ID for fields query: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
    }

    /// <summary>
    /// Gets a field by its identifier.
    /// </summary>
    [HttpGet("{objectId:int}/fields/{fieldId:int}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<DynamicObjectFieldResponse, DynamicObjectFieldDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<DynamicObjectFieldResponse, DynamicObjectFieldDetailsRelated>>> GetFieldById(int objectId, int fieldId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting field by ID: {FieldId} for object ID: {ObjectId}", fieldId, objectId);
        var field = await _fieldService.GetFieldByIdAsync(objectId, fieldId, cancellationToken);
        if (field == null)
        {
            return this.CreateNotFoundProblem($"Field with ID '{fieldId}' for object ID '{objectId}' not found.");
        }
        var response = await _fieldFactory.BuildDetailsAsync(field, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Creates a new field definition for the given dynamic object ID.
    /// </summary>
    [HttpPost("{objectId:int}/fields")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<DynamicObjectFieldResponse, DynamicObjectFieldDetailsRelated>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EntityWithRelatedResponse<DynamicObjectFieldResponse, DynamicObjectFieldDetailsRelated>>> CreateField(
        int objectId,
        [FromBody] DynamicObjectField request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var actor = this.GetCurrentUserId() ?? CommonConstant.DEFAULT_SYSTEM_USER;
            request.CreatedBy = actor;
            request.UpdatedBy = actor;

            _logger.LogInformation("Creating field for object ID '{ObjectId}'", objectId);
            var created = await _fieldService.AddFieldAsync(objectId, request, cancellationToken);
            var response = await _fieldFactory.BuildDetailsAsync(created, cancellationToken);
            return CreatedAtAction(nameof(GetFieldById), new { objectId = objectId, fieldId = created.Id }, response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in field creation: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid field creation request: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Field creation conflict: {Message}", ex.Message);
            return this.CreateConflictProblem(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing field definition.
    /// </summary>
    [HttpPut("{objectId:int}/fields/{fieldId:int}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<DynamicObjectFieldResponse, DynamicObjectFieldDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EntityWithRelatedResponse<DynamicObjectFieldResponse, DynamicObjectFieldDetailsRelated>>> UpdateField(
        int objectId,
        int fieldId,
        [FromBody] DynamicObjectField request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _fieldService.GetFieldByIdAsync(objectId, fieldId, cancellationToken);
        if (existing == null)
        {
            _logger.LogWarning("Field not found for update: {FieldId}", fieldId);
            return this.CreateNotFoundProblem($"Field with ID '{fieldId}' not found.");
        }

        try
        {
            // Enforce identity from route and preserve immutable fields as needed
            request.Id = fieldId;
            request.ObjectId = existing.ObjectId;
            request.CreatedBy = existing.CreatedBy; // preserve creator
            request.CreatedAt = existing.CreatedAt; // preserve created timestamp if used
            request.UpdatedBy = this.GetCurrentUserId() ?? CommonConstant.DEFAULT_SYSTEM_USER;

            _logger.LogInformation("Updating field '{FieldId}' for object ID '{ObjectId}'", fieldId, objectId);
            var updated = await _fieldService.UpdateFieldAsync(objectId, request, cancellationToken);
            var response = await _fieldFactory.BuildDetailsAsync(updated, cancellationToken);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in field update: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid field update request: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Field not found for update: {FieldId}", fieldId);
            return NotFound(new ProblemDetails
            {
                Title = "Field Not Found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Field update conflict: {Message}", ex.Message);
            return this.CreateConflictProblem(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a field definition by object ID and field ID.
    /// </summary>
    [HttpDelete("{objectId:int}/fields/{fieldId:int}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteField(int objectId, int fieldId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting field '{FieldId}' for object ID '{ObjectId}'", fieldId, objectId);
            var deleted = await _fieldService.DeleteFieldAsync(objectId, fieldId, cancellationToken);
            if (!deleted)
            {
                return this.CreateNotFoundProblem($"Field with ID '{fieldId}' for object ID '{objectId}' not found.");
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid delete request for field: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
    }
}
