namespace OXDesk.Core.Entities;

/// <summary>
/// Represents an entity that is scoped to a specific tenant.
/// </summary>
public interface ITenantScoped
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    Guid TenantId { get; set; }
}
