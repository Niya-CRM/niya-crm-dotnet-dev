using OXDesk.Core.Common;

namespace OXDesk.Core.AuditLogs.ChangeHistory;

/// <summary>
/// Repository interface for change history log data access operations.
/// </summary>
public interface IChangeHistoryLogRepository
{
    /// <summary>
    /// Gets a change history log by its unique identifier.
    /// </summary>
    /// <param name="id">The change history log identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The change history log if found, otherwise null.</returns>
    Task<ChangeHistoryLog?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets change history logs by optional filters. All filters are optional and can be combined.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="objectKey">The object key (entity type) (optional).</param>
    /// <param name="objectItemId">The object item ID (entity ID) (optional).</param>
    /// <param name="fieldName">The field name (optional).</param>
    /// <param name="createdBy">The user who created the change (optional).</param>
    /// <param name="startDate">The start date for filtering (optional).</param>
    /// <param name="endDate">The end date for filtering (optional).</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of change history logs matching the filters.</returns>
    Task<IEnumerable<ChangeHistoryLog>> GetChangeHistoryLogsAsync(
        Guid tenantId,
        string? objectKey = null,
        Guid? objectItemId = null,
        string? fieldName = null,
        Guid? createdBy = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all change history logs with pagination.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of change history logs.</returns>
    Task<IEnumerable<ChangeHistoryLog>> GetAllAsync(
        Guid tenantId,
        int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new change history log entry.
    /// </summary>
    /// <param name="changeHistoryLog">The change history log to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added change history log.</returns>
    /// <remarks>TenantId should be set in the changeHistoryLog entity before calling this method.</remarks>
    Task<ChangeHistoryLog> AddAsync(ChangeHistoryLog changeHistoryLog, CancellationToken cancellationToken = default);
}
