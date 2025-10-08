namespace OXDesk.Core.Common.DTOs;

/// <summary>
/// Base DTO for audited entities with integer identifier.
/// Includes creation and update audit information.
/// </summary>
public abstract class AuditedDto : BaseDto
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the entity.
    /// </summary>
    public Guid UpdatedBy { get; set; }
}

/// <summary>
/// Base DTO for audited entities with Guid identifier.
/// Includes creation and update audit information.
/// </summary>
public abstract class AuditedDtoGuid : BaseDtoGuid
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who last updated the entity.
    /// </summary>
    public Guid UpdatedBy { get; set; }
}
