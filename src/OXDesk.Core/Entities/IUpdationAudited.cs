namespace OXDesk.Core.Entities;

/// <summary>
/// Represents update auditing metadata for an entity.
/// </summary>
public interface IUpdationAudited
{
    /// <summary>
    /// Gets the UTC timestamp when the entity was last updated.
    /// </summary>
    DateTime UpdatedAt { get; }

    /// <summary>
    /// Gets the identifier of the principal that last updated the entity.
    /// </summary>
    Guid UpdatedBy { get; }
}
