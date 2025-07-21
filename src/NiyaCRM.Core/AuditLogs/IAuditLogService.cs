using NiyaCRM.Core.Common;

namespace NiyaCRM.Core.AuditLogs;

/// <summary>
/// Service interface for audit log business operations.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    /// <param name="module">The module/entity type.</param>
    /// <param name="event">The event/action type.</param>
    /// <param name="mappedId">The affected entity ID.</param>
    /// <param name="ip">The IP address.</param>
    /// <param name="data">The audit data.</param>
    /// <param name="createdBy">The user who performed the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created audit log entry.</returns>
    Task<AuditLog> CreateAuditLogAsync(string module, string @event, string mappedId, string ip, string data, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an audit log by its identifier.
    /// </summary>
    /// <param name="id">The audit log identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The audit log if found, otherwise null.</returns>
    Task<AuditLog?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by optional filters. All filters are optional and can be combined.
    /// </summary>
    /// <param name="module">The module/entity type (optional).</param>
    /// <param name="mappedId">The mapped entity ID (optional).</param>
    /// <param name="createdBy">The user who performed the action (optional).</param>
    /// <param name="startDate">The start date for filtering (optional).</param>
    /// <param name="endDate">The end date for filtering (optional).</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit logs matching the filters.</returns>
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        string? module = null,
        string? mappedId = null,
        string? createdBy = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
        int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets all audit logs with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of audit logs.</returns>
    Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default);


}
