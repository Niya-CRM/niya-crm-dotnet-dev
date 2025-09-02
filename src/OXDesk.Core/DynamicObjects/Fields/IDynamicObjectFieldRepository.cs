using OXDesk.Core.Common;

namespace OXDesk.Core.DynamicObjects.Fields;

/// <summary>
/// Repository interface for dynamic object field type data access operations.
/// </summary>
public interface IDynamicObjectFieldRepository
{
    /// <summary>
    /// Gets a field type by its unique identifier.
    /// </summary>
    /// <param name="id">The field type identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The field type if found; otherwise, null.</returns>
    /// <remarks>Tenant scoping is enforced by EF Core global query filters.</remarks>
    Task<DynamicObjectFieldType?> GetFieldTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all field types.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of field types.</returns>
    /// <remarks>Tenant scoping is enforced by EF Core global query filters.</remarks>
    Task<IEnumerable<DynamicObjectFieldType>> GetAllFieldTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all fields for a given dynamic object ID.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of fields for the object.</returns>
    /// <remarks>Tenant scoping is enforced by EF Core global query filters.</remarks>
    Task<IEnumerable<DynamicObjectField>> GetFieldsByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a field by its dynamic object ID and unique identifier.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="id">The field identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The field if found; otherwise, null.</returns>
    /// <remarks>Tenant scoping is enforced by EF Core global query filters.</remarks>
    Task<DynamicObjectField?> GetFieldByIdAsync(Guid objectId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new field for the given dynamic object.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="field">The field to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created field entity.</returns>
    /// <remarks>TenantId should be set in the field entity before calling this method.</remarks>
    Task<DynamicObjectField> AddFieldAsync(Guid objectId, DynamicObjectField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing field for the given dynamic object.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="field">The field to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated field entity.</returns>
    /// <remarks>Tenant scoping is enforced by EF Core global query filters.</remarks>
    Task<DynamicObjectField> UpdateFieldAsync(Guid objectId, DynamicObjectField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a field by dynamic object ID and field ID.
    /// </summary>
    /// <param name="objectId">The dynamic object ID.</param>
    /// <param name="fieldId">The field identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a field was deleted; otherwise, false.</returns>
    /// <remarks>Tenant scoping is enforced by EF Core global query filters.</remarks>
    Task<bool> DeleteFieldAsync(Guid objectId, Guid fieldId, CancellationToken cancellationToken = default);
}
