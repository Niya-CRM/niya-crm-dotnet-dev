using Microsoft.AspNetCore.Http;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.AuditLogs.DTOs;
using System.Linq;

namespace OXDesk.Application.AuditLogs
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;
        private readonly ICorrelationIdAccessor _correlationIdAccessor;

        public AuditLogService(
            IAuditLogRepository repository,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            _repository = repository;
            _correlationIdAccessor = correlationIdAccessor;
        }

        /// <inheritdoc/>
        public async Task<AuditLog> CreateAuditLogAsync(int objectId, string @event, int objectItemId, string ip, string data, int createdBy, CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLog(
                objectId,
                @event,
                objectItemId,
                ip,
                data,
                createdBy
            );

            // Set correlation ID from current request context
            auditLog.CorrelationId = _correlationIdAccessor.GetCorrelationId();

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
}
