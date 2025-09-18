using OXDesk.Core.Common;

namespace OXDesk.Core.DynamicObjects.Fields;

/// <summary>
/// Service interface for dynamic object field operations.
/// </summary>
public interface IDynamicObjectFieldService
{
    /// <summary>
    /// Gets a field type by its unique identifier.
    /// </summary>
    /// <param name="id">The field type identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The field type if found; otherwise, null.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<DynamicObjectFieldType?> GetFieldTypeByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all field types.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of field types.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<IEnumerable<DynamicObjectFieldType>> GetAllFieldTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all fields for a given dynamic object ID.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of fields for the object.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<IEnumerable<DynamicObjectField>> GetFieldsByObjectIdAsync(int objectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a field by its dynamic object ID and unique identifier.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="id">The field identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The field if found; otherwise, null.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<DynamicObjectField?> GetFieldByIdAsync(int objectId, int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new field for a given dynamic object ID.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="field">The field to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created field entity.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context and set on the field entity.</remarks>
    Task<DynamicObjectField> AddFieldAsync(int objectId, DynamicObjectField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing field for a given dynamic object ID.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="field">The field to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated field entity.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<DynamicObjectField> UpdateFieldAsync(int objectId, DynamicObjectField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a field by dynamic object ID and field ID.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="fieldId">The field identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a field was deleted; otherwise, false.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<bool> DeleteFieldAsync(int objectId, int fieldId, CancellationToken cancellationToken = default);
}
