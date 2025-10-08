namespace OXDesk.Core.Entities;

/// <summary>
/// Represents an entity with a Guid identifier.
/// </summary>
public interface IEntityGuid
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    Guid Id { get; set; }
}
