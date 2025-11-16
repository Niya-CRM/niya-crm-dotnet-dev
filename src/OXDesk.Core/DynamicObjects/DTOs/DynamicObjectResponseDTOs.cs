using System;
using OXDesk.Core.Common.DTOs;

namespace OXDesk.Core.DynamicObjects.DTOs
{
    /// <summary>
    /// Response DTO for dynamic object information.
    /// Inherits: Id (int), CreatedAt, CreatedBy, UpdatedAt, UpdatedBy from TenantScopedAuditedDto.
    /// </summary>
    public sealed class DynamicObjectResponse : TenantScopedAuditedDto
    {
        public string ObjectName { get; set; } = string.Empty;
        public string SingularName { get; set; } = string.Empty;
        public string PluralName { get; set; } = string.Empty;
        public string ObjectKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;
        public DateTime? DeletedAt { get; set; }
        
        // Enriched fields
        public string? CreatedByText { get; set; }
        public string? UpdatedByText { get; set; }
    }

    /// <summary>
    /// Related reference data for dynamic object details.
    /// </summary>
    public sealed class DynamicObjectDetailsRelated
    {
    }
}
