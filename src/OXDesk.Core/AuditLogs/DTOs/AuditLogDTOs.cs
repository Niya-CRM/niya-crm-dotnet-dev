namespace OXDesk.Core.AuditLogs.DTOs
{
    public sealed class AuditLogResponse
    {
        public int Id { get; set; }
        public string Event { get; set; } = string.Empty;
        public int ObjectId { get; set; }
        public Guid? ObjectItemIdUuid { get; set; }
        public int? ObjectItemIdInt { get; set; }
        public string IP { get; set; } = string.Empty;
        public string? Data { get; set; }
        public string? TraceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByText { get; set; }
    }
}
