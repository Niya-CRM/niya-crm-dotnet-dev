using OXDesk.Core.Common;

namespace OXDesk.Core.Tenants.DTOs;

public class TenantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    public Guid UserId { get; set; }
    public string? DatabaseName { get; set; }
    public string IsActive { get; set; } = "Y";
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid LastModifiedBy { get; set; }

    // Enriched
    public string? CreatedByText { get; set; }
    public string? LastModifiedByText { get; set; }
}

public class TenantDetailsRelated
{
}
