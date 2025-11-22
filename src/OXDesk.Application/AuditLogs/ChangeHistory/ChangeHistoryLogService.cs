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

namespace OXDesk.Application.AuditLogs.ChangeHistory
{
    /// <summary>
    /// Implementation of the change history log service.
    /// </summary>
    public class ChangeHistoryLogService : IChangeHistoryLogService
    {
        private readonly IChangeHistoryLogRepository _repository;
        private readonly ICorrelationIdAccessor _correlationIdAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeHistoryLogService"/> class.
        /// </summary>
        /// <param name="repository">The change history log repository.</param>
        /// <param name="correlationIdAccessor">The correlation ID accessor.</param>
        public ChangeHistoryLogService(
            IChangeHistoryLogRepository repository,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            _repository = repository;
            _correlationIdAccessor = correlationIdAccessor;
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

            // Set correlation ID from current request context
            changeHistoryLog.CorrelationId = _correlationIdAccessor.GetCorrelationId();

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
}

