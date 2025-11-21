using OXDesk.Core.Common;

namespace OXDesk.Core.AuditLogs.DTOs
{
    /// <summary>
    /// DTO for audit log query parameters.
    /// </summary>
    public class AuditLogQueryDto
    {
        public int? ObjectId { get; set; }
        public Guid? ObjectItemIdUuid { get; set; }
        public int? ObjectItemIdInt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = CommonConstant.PAGE_NUMBER_DEFAULT;
        public int PageSize { get; set; } = CommonConstant.PAGE_SIZE_DEFAULT;
    }
}

