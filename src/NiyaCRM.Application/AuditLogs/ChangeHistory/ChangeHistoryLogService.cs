using NiyaCRM.Core.AuditLogs.ChangeHistory;
using NiyaCRM.Core.AuditLogs.ChangeHistory.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NiyaCRM.Application.AuditLogs.ChangeHistory
{
    /// <summary>
    /// Implementation of the change history log service.
    /// </summary>
    public class ChangeHistoryLogService : IChangeHistoryLogService
    {
        private readonly IChangeHistoryLogRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeHistoryLogService"/> class.
        /// </summary>
        /// <param name="repository">The change history log repository.</param>
        public ChangeHistoryLogService(IChangeHistoryLogRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public async Task<ChangeHistoryLog> CreateChangeHistoryLogAsync(
            string objectKey,
            Guid objectItemId,
            string fieldName,
            string? oldValue,
            string? newValue,
            Guid createdBy,
            CancellationToken cancellationToken = default)
        {
            var changeHistoryLog = new ChangeHistoryLog(
                Guid.CreateVersion7(),
                objectKey,
                objectItemId,
                fieldName,
                oldValue,
                newValue,
                createdBy
            );

            return await _repository.AddAsync(changeHistoryLog, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ChangeHistoryLog?> GetChangeHistoryLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChangeHistoryLog>> GetChangeHistoryLogsAsync(
            ChangeHistoryLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            return await _repository.GetChangeHistoryLogsAsync(
                query.ObjectKey,
                query.ObjectItemId,
                query.FieldName,
                query.CreatedBy,
                query.StartDate,
                query.EndDate,
                query.PageNumber,
                query.PageSize,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChangeHistoryLog>> GetAllChangeHistoryLogsAsync(
            int pageNumber = Core.Common.CommonConstant.PAGE_NUMBER_DEFAULT,
            int pageSize = Core.Common.CommonConstant.PAGE_SIZE_DEFAULT,
            CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}
