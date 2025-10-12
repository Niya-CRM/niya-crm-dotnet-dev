using System;

namespace OXDesk.Core.AuditLogs.ChangeHistory.DTOs
{
    public sealed class ChangeHistoryLogResponse
    {
        public Guid Id { get; set; }
        public string ObjectKey { get; set; } = string.Empty;
        public Guid? ObjectItemIdUuid { get; set; }
        public int? ObjectItemIdInt { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public string? CreatedByText { get; set; }
    }
}
