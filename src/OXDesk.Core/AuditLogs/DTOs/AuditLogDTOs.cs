namespace OXDesk.Core.AuditLogs.DTOs
{
    public sealed class AuditLogResponse
    {
        public int Id { get; set; }
        public Guid TenantId { get; set; }
        public string ObjectKey { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public Guid ObjectItemId { get; set; }
        public string IP { get; set; } = string.Empty;
        public string? Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByText { get; set; }
    }
}
