namespace OXDesk.Core.AuditLogs.DTOs
{
    public sealed class AuditLogResponse
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string ObjectKey { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public string ObjectItemId { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public string? Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public string? CreatedByText { get; set; }
    }
}
