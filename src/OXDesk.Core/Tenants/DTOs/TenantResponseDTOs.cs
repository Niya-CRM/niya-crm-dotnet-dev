using OXDesk.Core.Common;
using OXDesk.Core.Common.DTOs;

namespace OXDesk.Core.Tenants.DTOs;

/// <summary>
/// Response DTO for tenant information.
/// Inherits: Id (Guid), CreatedAt, CreatedBy, UpdatedAt, UpdatedBy from AuditedDtoGuid.
/// </summary>
public class TenantResponse : AuditedDtoGuid
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    public Guid UserId { get; set; }
    public string? DatabaseName { get; set; }
    public string IsActive { get; set; } = "Y";
    public DateTime? DeletedAt { get; set; }

    // Enriched fields
    public string? CreatedByText { get; set; }
    public string? UpdatedByText { get; set; }
}

/// <summary>
/// Related reference data for tenant details.
/// </summary>
public class TenantDetailsRelated
{
}
