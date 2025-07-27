using NiyaCRM.Core.Common;

namespace NiyaCRM.Core.AuditLogs.DTOs
{
    /// <summary>
    /// DTO for audit log query parameters.
    /// </summary>
    public class AuditLogQueryDto
    {
        public string? Module { get; set; }
        public string? MappedId { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = CommonConstant.PAGE_NUMBER_DEFAULT;
        public int PageSize { get; set; } = CommonConstant.PAGE_SIZE_DEFAULT;
    }
}

