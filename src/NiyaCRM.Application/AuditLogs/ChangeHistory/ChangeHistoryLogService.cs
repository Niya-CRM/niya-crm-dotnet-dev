using NiyaCRM.Core.AuditLogs.ChangeHistory;
using NiyaCRM.Core.AuditLogs.ChangeHistory.DTOs;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.Common.Response;
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
        public async Task<IEnumerable<EntityDto>> GetChangeHistoryLogsAsync(
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
            var result = new List<EntityDto>();
            
            foreach (var log in logs)
            {
                var userFullName = await _userService.GetUserFullNameFromCacheAsync(log.CreatedBy, cancellationToken);
                
                var entity = new EntityDto();
                entity.Fields["Id"] = new FieldDto
                {
                    FieldKey = "Id",
                    FieldLabel = "Id",
                    FieldValue = log.Id.ToString(),
                    DisplayValue = log.Id.ToString()
                };
                entity.Fields["ObjectKey"] = new FieldDto
                {
                    FieldKey = "ObjectKey",
                    FieldLabel = "Object Key",
                    FieldValue = log.ObjectKey,
                    DisplayValue = log.ObjectKey
                };
                entity.Fields["ObjectItemId"] = new FieldDto
                {
                    FieldKey = "ObjectItemId",
                    FieldLabel = "Object Item Id",
                    FieldValue = log.ObjectItemId.ToString(),
                    DisplayValue = log.ObjectItemId.ToString()
                };
                entity.Fields["FieldName"] = new FieldDto
                {
                    FieldKey = "FieldName",
                    FieldLabel = "Field Name",
                    FieldValue = log.FieldName,
                    DisplayValue = log.FieldName
                };
                entity.Fields["OldValue"] = new FieldDto
                {
                    FieldKey = "OldValue",
                    FieldLabel = "Old Value",
                    FieldValue = log.OldValue ?? string.Empty,
                    DisplayValue = log.OldValue ?? string.Empty
                };
                entity.Fields["NewValue"] = new FieldDto
                {
                    FieldKey = "NewValue",
                    FieldLabel = "New Value",
                    FieldValue = log.NewValue ?? string.Empty,
                    DisplayValue = log.NewValue ?? string.Empty
                };
                entity.Fields["CreatedAt"] = new FieldDto
                {
                    FieldKey = "CreatedAt",
                    FieldLabel = "Created At",
                    FieldValue = log.CreatedAt.ToString("o"),
                    DisplayValue = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                };
                entity.Fields["CreatedBy"] = new FieldDto
                {
                    FieldKey = "CreatedBy",
                    FieldLabel = "Created By",
                    FieldValue = log.CreatedBy.ToString(),
                    DisplayValue = userFullName
                };

                result.Add(entity);
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
