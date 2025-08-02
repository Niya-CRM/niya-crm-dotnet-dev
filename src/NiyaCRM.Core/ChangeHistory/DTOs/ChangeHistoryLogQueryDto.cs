using NiyaCRM.Core.Common;

namespace NiyaCRM.Core.ChangeHistory.DTOs
{
    /// <summary>
    /// DTO for change history log query parameters.
    /// </summary>
    public class ChangeHistoryLogQueryDto
    {
        public string? ObjectKey { get; set; }
        public Guid? ObjectItemId { get; set; }
        public string? FieldName { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = CommonConstant.PAGE_NUMBER_DEFAULT;
        public int PageSize { get; set; } = CommonConstant.PAGE_SIZE_DEFAULT;
    }
}
