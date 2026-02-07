using System;

namespace OXDesk.Core.AuditLogs.ChangeHistory.DTOs
{
    public sealed class ChangeHistoryLogResponse
    {
        public int Id { get; set; }
        public int ObjectId { get; set; }
        public Guid? ObjectItemIdUuid { get; set; }
        public int? ObjectItemIdInt { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? TraceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByText { get; set; }
    }
}
