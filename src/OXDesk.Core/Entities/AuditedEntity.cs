namespace OXDesk.Core.Entities;

/// <summary>
/// Provides common auditing properties for entities that track creation and update metadata.
/// </summary>
public class AuditedEntity : ICreationAudited, IUpdationAudited
{
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public int CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }

    /// <inheritdoc />
    public int UpdatedBy { get; set; }
}
