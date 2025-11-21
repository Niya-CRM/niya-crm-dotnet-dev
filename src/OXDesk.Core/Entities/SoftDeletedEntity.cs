namespace OXDesk.Core.Entities;

/// <summary>
/// Provides soft deletion properties for an entity.
/// </summary>
public class SoftDeletedEntity : ISoftDelete
{
    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <inheritdoc />
    public int? DeletedBy { get; set; }
}
