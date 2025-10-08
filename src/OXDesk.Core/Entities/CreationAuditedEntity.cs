namespace OXDesk.Core.Entities;

/// <summary>
/// Provides creation auditing properties for an entity.
/// </summary>
public class CreationAuditedEntity : ICreationAudited
{
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public Guid CreatedBy { get; set; }
}