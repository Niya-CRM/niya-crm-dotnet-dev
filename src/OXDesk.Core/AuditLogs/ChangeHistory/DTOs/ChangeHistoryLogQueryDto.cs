using OXDesk.Core.Common;

namespace OXDesk.Core.AuditLogs.ChangeHistory.DTOs
{
    /// <summary>
    /// DTO for change history log query parameters.
    /// </summary>
    public class ChangeHistoryLogQueryDto
    {
        public string? ObjectKey { get; set; }
        public Guid? ObjectItemIdUuid { get; set; }
        public int? ObjectItemIdInt { get; set; }
        public string? FieldName { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = CommonConstant.PAGE_NUMBER_DEFAULT;
        public int PageSize { get; set; } = CommonConstant.PAGE_SIZE_DEFAULT;
    }
}
