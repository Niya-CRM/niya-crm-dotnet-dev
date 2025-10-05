using OXDesk.Core.Common;
using OXDesk.Core.AuditLogs.DTOs;

namespace OXDesk.Core.AuditLogs;

/// <summary>
/// Service interface for audit log business operations.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    /// <param name="objectKey">The object key/entity type.</param>
    /// <param name="event">The event/action type.</param>
    /// <param name="objectItemId">The affected entity ID.</param>
    /// <param name="ip">The IP address.</param>
    /// <param name="data">The audit data.</param>
    /// <param name="createdBy">The user who performed the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created audit log entry.</returns>
    Task<AuditLog> CreateAuditLogAsync(string objectKey, string @event, Guid objectItemId, string ip, string data, int createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an audit log by its identifier.
    /// </summary>
    /// <param name="id">The audit log identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The audit log if found, otherwise null.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<AuditLog?> GetAuditLogByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by optional filters. All filters are optional and can be combined.
    /// </summary>
    /// <param name="query">The query parameters for filtering audit logs.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit logs matching the filters.</returns>
    /// <remarks>TenantId is automatically retrieved from the current context.</remarks>
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        AuditLogQueryDto query,
        CancellationToken cancellationToken = default
    );


}
