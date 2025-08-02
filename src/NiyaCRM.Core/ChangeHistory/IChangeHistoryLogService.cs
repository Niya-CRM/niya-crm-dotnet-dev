using NiyaCRM.Core.Common;

namespace NiyaCRM.Core.ChangeHistory;

/// <summary>
/// Service interface for change history log business operations.
/// </summary>
public interface IChangeHistoryLogService
{
    /// <summary>
    /// Creates a new change history log entry.
    /// </summary>
    /// <param name="objectKey">The object key (entity type).</param>
    /// <param name="objectItemId">The object item ID (entity ID).</param>
    /// <param name="fieldName">The name of the field that was changed.</param>
    /// <param name="oldValue">The previous value of the field.</param>
    /// <param name="newValue">The new value of the field.</param>
    /// <param name="createdBy">The user who created the change.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created change history log entry.</returns>
    Task<ChangeHistoryLog> CreateChangeHistoryLogAsync(
        string objectKey,
        Guid objectItemId,
        string fieldName,
        string? oldValue,
        string? newValue,
        Guid createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a change history log by its identifier.
    /// </summary>
    /// <param name="id">The change history log identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The change history log if found, otherwise null.</returns>
    Task<ChangeHistoryLog?> GetChangeHistoryLogByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets change history logs by optional filters. All filters are optional and can be combined.
    /// </summary>
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
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of change history logs.</returns>
    Task<IEnumerable<ChangeHistoryLog>> GetAllChangeHistoryLogsAsync(
        int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default);
}
