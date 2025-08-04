using NiyaCRM.Core.AuditLogs.ChangeHistory;
using NiyaCRM.Core.AuditLogs.ChangeHistory.DTOs;
using NiyaCRM.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NiyaCRM.Core.Identity;

namespace NiyaCRM.Application.AuditLogs.ChangeHistory
{
    /// <summary>
    /// Implementation of the change history log service.
    /// </summary>
    public class ChangeHistoryLogService : IChangeHistoryLogService
    {
        private readonly IChangeHistoryLogRepository _repository;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeHistoryLogService"/> class.
        /// </summary>
        /// <param name="repository">The change history log repository.</param>
        public ChangeHistoryLogService(IChangeHistoryLogRepository repository, IUserService userService)
        {
            _repository = repository;
            _userService = userService;
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
        public async Task<IEnumerable<ChangeHistoryLogResponseWithDisplay>> GetChangeHistoryLogsAsync(
            ChangeHistoryLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var logs = await _repository.GetChangeHistoryLogsAsync(
                query.ObjectKey,
                query.ObjectItemId,
                query.FieldName,
                query.CreatedBy,
                query.StartDate,
                query.EndDate,
                query.PageNumber,
                query.PageSize,
                cancellationToken);
                
            // Convert to response with display values
            var result = new List<ChangeHistoryLogResponseWithDisplay>();
            
            foreach (var log in logs)
            {
                var userFullName = await _userService.GetUserFullNameFromCacheAsync(log.CreatedBy, cancellationToken);
                
                result.Add(new ChangeHistoryLogResponseWithDisplay
                {
                    Id = new ValueDisplayPair<Guid> { Value = log.Id, DisplayValue = log.Id.ToString() },
                    ObjectKey = new ValueDisplayPair<string> { Value = log.ObjectKey, DisplayValue = log.ObjectKey },
                    ObjectItemId = new ValueDisplayPair<Guid> { Value = log.ObjectItemId, DisplayValue = log.ObjectItemId.ToString() },
                    FieldName = new ValueDisplayPair<string> { Value = log.FieldName, DisplayValue = log.FieldName },
                    OldValue = new ValueDisplayPair<string> { Value = log.OldValue, DisplayValue = log.OldValue ?? string.Empty },
                    NewValue = new ValueDisplayPair<string> { Value = log.NewValue, DisplayValue = log.NewValue ?? string.Empty },
                    CreatedAt = new ValueDisplayPair<DateTime> { Value = log.CreatedAt, DisplayValue = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") },
                    CreatedBy = new ValueDisplayPair<Guid> { Value = log.CreatedBy, DisplayValue = userFullName }
                });
            }
            
            return result;
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
