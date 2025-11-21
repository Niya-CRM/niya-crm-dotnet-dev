namespace OXDesk.Core.Entities;

/// <summary>
/// Provides auditing metadata for creation, updates, and soft deletion of an entity.
/// </summary>
public class AuditedEntityWithSoftDelete : ICreationAudited, IUpdationAudited, ISoftDelete
{
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public int CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <inheritdoc />
    public int? DeletedBy { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }

    /// <inheritdoc />
    public int UpdatedBy { get; set; }
}
