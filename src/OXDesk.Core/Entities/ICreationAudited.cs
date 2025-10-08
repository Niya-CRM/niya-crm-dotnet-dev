namespace OXDesk.Core.Entities;

/// <summary>
/// Represents creation auditing metadata for an entity.
/// </summary>
public interface ICreationAudited
{
    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the principal that created the entity.
    /// </summary>
    Guid CreatedBy { get; set; }
}