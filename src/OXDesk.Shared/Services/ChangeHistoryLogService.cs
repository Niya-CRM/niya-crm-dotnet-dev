using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using OXDesk.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace OXDesk.Shared.Services;

/// <summary>
/// Implementation of the change history log service.
/// </summary>
public class ChangeHistoryLogService : IChangeHistoryLogService
{
    private readonly IChangeHistoryLogRepository _repository;
    private readonly ITraceIdAccessor _traceIdAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeHistoryLogService"/> class.
    /// </summary>
    /// <param name="repository">The change history log repository.</param>
    /// <param name="traceIdAccessor">The trace ID accessor.</param>
    public ChangeHistoryLogService(
        IChangeHistoryLogRepository repository,
        ITraceIdAccessor traceIdAccessor)
    {
        _repository = repository;
        _traceIdAccessor = traceIdAccessor;
    }

    /// <inheritdoc/>
    public async Task<ChangeHistoryLog> CreateChangeHistoryLogAsync(
        int objectId,
        int objectItemId,
        string fieldName,
        string? oldValue,
        string? newValue,
        int createdBy,
        CancellationToken cancellationToken = default)
    {
        var changeHistoryLog = new ChangeHistoryLog(
            objectId,
            objectItemId,
            fieldName,
            oldValue,
            newValue,
            createdBy
        );

        changeHistoryLog.TraceId = _traceIdAccessor.GetTraceId();

        return await _repository.AddAsync(changeHistoryLog, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeHistoryLog> CreateChangeHistoryLogAsync(
        int objectId,
        Guid objectItemId,
        string fieldName,
        string? oldValue,
        string? newValue,
        int createdBy,
        CancellationToken cancellationToken = default)
    {
        var changeHistoryLog = new ChangeHistoryLog(
            objectId,
            objectItemId,
            fieldName,
            oldValue,
            newValue,
            createdBy
        );

        changeHistoryLog.TraceId = _traceIdAccessor.GetTraceId();

        return await _repository.AddAsync(changeHistoryLog, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ChangeHistoryLog?> GetChangeHistoryLogByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ChangeHistoryLog>> GetChangeHistoryLogsAsync(
        ChangeHistoryLogQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var logs = await _repository.GetChangeHistoryLogsAsync(
            query,
            cancellationToken);
        return logs;
    }
}
