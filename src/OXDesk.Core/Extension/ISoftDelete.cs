namespace OXDesk.Core.Extension;

/// <summary>
/// Represents soft deletion metadata for an entity.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was soft deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the principal that performed the soft delete.
    /// </summary>
    Guid? DeletedBy { get; set; }
}