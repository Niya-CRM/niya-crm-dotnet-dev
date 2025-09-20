using OXDesk.Core.Common;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;

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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The change history log if found, otherwise null.</returns>
    Task<ChangeHistoryLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets change history logs by optional filters. All filters are optional and can be combined.
    /// </summary>
    /// <param name="query">Query object containing filter and pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of change history logs matching the filters.</returns>
    Task<IEnumerable<ChangeHistoryLog>> GetChangeHistoryLogsAsync(
        ChangeHistoryLogQueryDto query,
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

