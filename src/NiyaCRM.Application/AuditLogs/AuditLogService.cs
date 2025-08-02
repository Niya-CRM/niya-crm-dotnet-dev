using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Core.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NiyaCRM.Core.AuditLogs.DTOs;

namespace NiyaCRM.Application.AuditLogs
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;

        public AuditLogService(IAuditLogRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public async Task<AuditLog> CreateAuditLogAsync(string objectKey, string @event, string objectItemId, string ip, string data, Guid createdBy, CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLog(
                Guid.NewGuid(),
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
        public async Task<AuditLog?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            AuditLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            return await _repository.GetAuditLogsAsync(
                query.ObjectKey,
                query.ObjectItemId,
                query.CreatedBy,
                query.StartDate,
                query.EndDate,
                query.PageNumber,
                query.PageSize,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}
