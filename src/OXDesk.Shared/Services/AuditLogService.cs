using Microsoft.AspNetCore.Http;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.AuditLogs.DTOs;
using System.Linq;

namespace OXDesk.Shared.Services;

/// <summary>
/// Service for managing audit logs.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly ITraceIdAccessor _traceIdAccessor;

    public AuditLogService(
        IAuditLogRepository repository,
        ITraceIdAccessor traceIdAccessor)
    {
        _repository = repository;
        _traceIdAccessor = traceIdAccessor;
    }

    /// <inheritdoc/>
    public async Task<AuditLog> CreateAuditLogAsync(string @event, int objectId, Guid objectItemId, string ip, string data, int createdBy, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog(
            @event,
            objectId,
            objectItemId,
            ip,
            data,
            createdBy
        );

        auditLog.TraceId = _traceIdAccessor.GetTraceId();

        return await _repository.AddAsync(auditLog, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AuditLog> CreateAuditLogAsync(string @event, int objectId, int objectItemId, string ip, string data, int createdBy, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog(
            @event,
            objectId,
            objectItemId,
            ip,
            data,
            createdBy
        );

        auditLog.TraceId = _traceIdAccessor.GetTraceId();

        return await _repository.AddAsync(auditLog, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AuditLog?> GetAuditLogByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        AuditLogQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var logs = await _repository.GetAuditLogsAsync(
            query,
            cancellationToken);
        return logs;
    }
}
