using OXDesk.Core.Common;
using OXDesk.Core.AuditLogs.DTOs;

namespace OXDesk.Core.AuditLogs;

/// <summary>
/// Repository interface for audit log data access operations.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets an audit log by its unique identifier.
    /// </summary>
    /// <param name="id">The audit log identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The audit log if found, otherwise null.</returns>
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by optional filters. All filters are optional and can be combined.
    /// </summary>
    /// <param name="query">The query parameters for filtering audit logs.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit logs matching the filters.</returns>
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        AuditLogQueryDto query,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new audit log entry.
    /// </summary>
    /// <param name="auditLog">The audit log to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added audit log.</returns>
    Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);


}
