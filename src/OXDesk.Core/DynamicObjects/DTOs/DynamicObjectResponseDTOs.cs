using System;
using OXDesk.Core.Common.DTOs;

namespace OXDesk.Core.DynamicObjects.DTOs
{
    public sealed class DynamicObjectResponse : TenantScopedAuditedDto
    {
        // Inherits: Id (int), TenantId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
        
        public string ObjectName { get; set; } = string.Empty;
        public string SingularName { get; set; } = string.Empty;
        public string PluralName { get; set; } = string.Empty;
        public string ObjectKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;
        public DateTime? DeletedAt { get; set; }
        public string? CreatedByText { get; set; }
        public string? UpdatedByText { get; set; }
    }

    public sealed class DynamicObjectDetailsRelated
    {
    }
}
