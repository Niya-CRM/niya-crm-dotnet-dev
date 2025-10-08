namespace OXDesk.Core.Entities;

/// <summary>
/// Represents an entity with an integer identifier.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    int Id { get; set; }
}
