namespace OXDesk.Core.Common.DTOs;

/// <summary>
/// Base DTO for entities with integer identifier.
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Base DTO for entities with Guid identifier.
/// </summary>
public abstract class BaseDtoGuid
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
}
