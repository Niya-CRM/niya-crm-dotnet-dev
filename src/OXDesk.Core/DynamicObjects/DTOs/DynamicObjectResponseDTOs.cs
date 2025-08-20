using System;

namespace OXDesk.Core.DynamicObjects.DTOs
{
    public sealed class DynamicObjectResponse
    {
        public Guid Id { get; set; }
        public string ObjectName { get; set; } = string.Empty;
        public string SingularName { get; set; } = string.Empty;
        public string PluralName { get; set; } = string.Empty;
        public string ObjectKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public string? CreatedByText { get; set; }
        public string? UpdatedByText { get; set; }
    }

    public sealed class DynamicObjectDetailsRelated
    {
    }
}
