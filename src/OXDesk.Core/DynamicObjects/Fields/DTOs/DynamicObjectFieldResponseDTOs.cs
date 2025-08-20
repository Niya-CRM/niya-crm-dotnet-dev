using System;

namespace OXDesk.Core.DynamicObjects.Fields.DTOs
{
    /// <summary>
    /// Response DTO for DynamicObjectField enriched for API responses.
    /// </summary>
    public sealed class DynamicObjectFieldResponse
    {
        public Guid Id { get; set; }
        public string ObjectKey { get; set; } = string.Empty;
        public string FieldKey { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public bool Indexed { get; set; }
        public string? Description { get; set; }
        public string? HelpText { get; set; }
        public string? Placeholder { get; set; }
        public bool Required { get; set; }
        public bool Unique { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public int? Decimals { get; set; }
        public int? MaxFileSize { get; set; }
        public string? AllowedFileTypes { get; set; }
        public int? MinFileCount { get; set; }
        public int? MaxFileCount { get; set; }
        public Guid? ValueListId { get; set; }
        public int? MinSelectedItems { get; set; }
        public int? MaxSelectedItems { get; set; }
        public bool EditableAfterSubmission { get; set; }
        public bool VisibleOnCreate { get; set; }
        public bool VisibleOnEdit { get; set; }
        public bool VisibleOnView { get; set; }
        public bool AuditChanges { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }

        // Enriched fields
        public string? CreatedByText { get; set; }
        public string? UpdatedByText { get; set; }
    }

    /// <summary>
    /// Related payload for DynamicObjectField details endpoints.
    /// Reserved for future related data (e.g., value list items).
    /// </summary>
    public sealed class DynamicObjectFieldDetailsRelated
    {
    }
}
