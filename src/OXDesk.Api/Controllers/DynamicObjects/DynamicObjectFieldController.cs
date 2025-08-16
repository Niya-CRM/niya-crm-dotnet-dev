using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Api.Common;
using OXDesk.Core.Common;
using OXDesk.Core.DynamicObjects.Fields;
using System.ComponentModel.DataAnnotations;
using System;

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

    public DynamicObjectFieldController(
        IDynamicObjectFieldService fieldService,
        ILogger<DynamicObjectFieldController> logger)
    {
        _fieldService = fieldService ?? throw new ArgumentNullException(nameof(fieldService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Field Types

    /// <summary>
    /// Gets all available field types.
    /// </summary>
    [HttpGet("field-types")]
    [ProducesResponseType(typeof(IEnumerable<DynamicObjectFieldType>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DynamicObjectFieldType>>> GetAllFieldTypes(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all DynamicObject field types");
        var types = await _fieldService.GetAllFieldTypesAsync(cancellationToken);
        return Ok(types);
    }

    /// <summary>
    /// Gets a field type by its identifier.
    /// </summary>
    [HttpGet("field-types/{fieldTypeId:guid}")]
    [ProducesResponseType(typeof(DynamicObjectFieldType), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DynamicObjectFieldType>> GetFieldTypeById(Guid fieldTypeId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting field type by ID: {FieldTypeId}", fieldTypeId);
        var type = await _fieldService.GetFieldTypeByIdAsync(fieldTypeId, cancellationToken);
        if (type == null)
        {
            return this.CreateNotFoundProblem($"Field type with ID '{fieldTypeId}' not found.");
        }
        return Ok(type);
    }

    // Fields

    /// <summary>
    /// Lists all fields for a given dynamic object ID.
    /// </summary>
    [HttpGet("{objectId:guid}/fields")]
    [ProducesResponseType(typeof(IEnumerable<DynamicObjectField>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<DynamicObjectField>>> GetFieldsByObjectId(Guid objectId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting fields for object ID: {ObjectId}", objectId);
            var fields = await _fieldService.GetFieldsByObjectIdAsync(objectId, cancellationToken);
            return Ok(fields);
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
    [HttpGet("{objectId:guid}/fields/{fieldId:guid}")]
    [ProducesResponseType(typeof(DynamicObjectField), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DynamicObjectField>> GetFieldById(Guid objectId, Guid fieldId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting field by ID: {FieldId} for object ID: {ObjectId}", fieldId, objectId);
        var field = await _fieldService.GetFieldByIdAsync(objectId, fieldId, cancellationToken);
        if (field == null)
        {
            return this.CreateNotFoundProblem($"Field with ID '{fieldId}' for object ID '{objectId}' not found.");
        }
        return Ok(field);
    }

    /// <summary>
    /// Creates a new field definition for the given dynamic object ID.
    /// </summary>
    [HttpPost("{objectId:guid}/fields")]
    [ProducesResponseType(typeof(DynamicObjectField), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DynamicObjectField>> CreateField(
        Guid objectId,
        [FromBody] DynamicObjectField request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            request.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id; // Allow client to omit ID
            request.CreatedBy = CommonConstant.DEFAULT_SYSTEM_USER;
            request.UpdatedBy = CommonConstant.DEFAULT_SYSTEM_USER;

            _logger.LogInformation("Creating field '{FieldKey}' for object ID '{ObjectId}'", request.FieldKey, objectId);
            var created = await _fieldService.AddFieldAsync(objectId, request, cancellationToken);

            return CreatedAtAction(nameof(GetFieldById), new { objectId = objectId, fieldId = created.Id }, created);
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
    [HttpPut("{objectId:guid}/fields/{fieldId:guid}")]
    [ProducesResponseType(typeof(DynamicObjectField), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DynamicObjectField>> UpdateField(
        Guid objectId,
        Guid fieldId,
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
            request.ObjectKey = existing.ObjectKey;
            request.CreatedBy = existing.CreatedBy; // preserve creator
            request.CreatedAt = existing.CreatedAt; // preserve created timestamp if used
            request.UpdatedBy = CommonConstant.DEFAULT_SYSTEM_USER;

            _logger.LogInformation("Updating field '{FieldId}' for object ID '{ObjectId}'", fieldId, objectId);
            var updated = await _fieldService.UpdateFieldAsync(objectId, request, cancellationToken);
            return Ok(updated);
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
    [HttpDelete("{objectId:guid}/fields/{fieldId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteField(Guid objectId, Guid fieldId, CancellationToken cancellationToken = default)
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
