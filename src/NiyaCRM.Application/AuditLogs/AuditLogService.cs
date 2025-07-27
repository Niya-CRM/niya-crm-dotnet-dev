using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NiyaCRM.Application.AuditLogs
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;

        public AuditLogService(IAuditLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<AuditLog> CreateAuditLogAsync(string module, string @event, string mappedId, string ip, string data, Guid createdBy, CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLog(
                Guid.NewGuid(),
                module,
                @event,
                mappedId,
                ip,
                data,
                DateTime.UtcNow,
                createdBy
            );
            return await _repository.AddAsync(auditLog, cancellationToken);
        }

        public async Task<AuditLog?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            string? module = null,
            string? mappedId = null,
            Guid? createdBy = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
            int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
            CancellationToken cancellationToken = default)
        {
            return await _repository.GetAuditLogsAsync(module, mappedId, createdBy, startDate, endDate, pageNumber, pageSize, cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}
