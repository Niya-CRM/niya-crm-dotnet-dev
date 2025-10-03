namespace OXDesk.Core.Extension;

/// <summary>
/// Provides soft deletion properties for an entity.
/// </summary>
public class SoftDeletedEntity : ISoftDelete
{
    /// <inheritdoc />
    public DateTime? DeletedAt { get; set; }

    /// <inheritdoc />
    public Guid? DeletedBy { get; set; }
}
