using NiyaCRM.Core.Common;

namespace NiyaCRM.Core.AuditLogs;

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
    Task<IEnumerable<AuditLog>> GetAllAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new audit log entry.
    /// </summary>
    /// <param name="auditLog">The audit log to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The added audit log.</returns>
    Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);


}
