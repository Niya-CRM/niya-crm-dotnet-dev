using NiyaCRM.Core.Common;

namespace NiyaCRM.Core.DynamicObjects.Fields;

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
    Task<DynamicObjectFieldType?> GetFieldTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all field types.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of field types.</returns>
    Task<IEnumerable<DynamicObjectFieldType>> GetAllFieldTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all fields for a given object key.
    /// </summary>
    /// <param name="objectKey">The dynamic object key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of fields for the object.</returns>
    Task<IEnumerable<DynamicObjectField>> GetFieldsByObjectKeyAsync(string objectKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a field by its unique identifier.
    /// </summary>
    /// <param name="id">The field identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The field if found; otherwise, null.</returns>
    Task<DynamicObjectField?> GetFieldByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new field.
    /// </summary>
    /// <param name="field">The field to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created field entity.</returns>
    Task<DynamicObjectField> AddFieldAsync(DynamicObjectField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing field.
    /// </summary>
    /// <param name="field">The field to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated field entity.</returns>
    Task<DynamicObjectField> UpdateFieldAsync(DynamicObjectField field, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a field by object key and field ID.
    /// </summary>
    /// <param name="objectKey">The dynamic object key.</param>
    /// <param name="fieldId">The field identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if a field was deleted; otherwise, false.</returns>
    Task<bool> DeleteFieldAsync(string objectKey, Guid fieldId, CancellationToken cancellationToken = default);
}
