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

        public AuditLogService(
            IAuditLogRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public async Task<AuditLog> CreateAuditLogAsync(string objectKey, string @event, Guid objectItemId, string ip, string data, Guid createdBy, CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLog(
                objectKey,
                @event,
                objectItemId,
                ip,
                data,
                createdBy
            );
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
