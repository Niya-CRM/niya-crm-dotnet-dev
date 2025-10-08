namespace OXDesk.Core.Common.DTOs;

/// <summary>
/// Base DTO for tenant-scoped entities with integer identifier.
/// </summary>
public abstract class TenantScopedDto : BaseDto
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }
}

/// <summary>
/// Base DTO for tenant-scoped entities with Guid identifier.
/// </summary>
public abstract class TenantScopedDtoGuid : BaseDtoGuid
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }
}

/// <summary>
/// Base DTO for tenant-scoped audited entities with integer identifier.
/// Combines tenant scoping with audit information.
/// </summary>
public abstract class TenantScopedAuditedDto : AuditedDto
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }
}

/// <summary>
/// Base DTO for tenant-scoped audited entities with Guid identifier.
/// Combines tenant scoping with audit information.
/// </summary>
public abstract class TenantScopedAuditedDtoGuid : AuditedDtoGuid
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }
}
