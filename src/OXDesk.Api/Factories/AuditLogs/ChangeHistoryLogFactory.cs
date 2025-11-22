using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.Common.Response;

namespace OXDesk.Api.Factories.AuditLogs
{
    /// <summary>
    /// Builds ChangeHistoryLog response DTOs and wraps them with related data.
    /// </summary>
    public sealed class ChangeHistoryLogFactory : IChangeHistoryLogFactory
    {
        private readonly IUserService _userService;

        public ChangeHistoryLogFactory(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private static ChangeHistoryLogResponse Map(ChangeHistoryLog log) => new()
        {
            Id = log.Id,
            ObjectId = log.ObjectId,
            ObjectItemIdInt = log.ObjectItemIdInt,
            FieldName = log.FieldName,
            OldValue = log.OldValue,
            NewValue = log.NewValue,
            CorrelationId = log.CorrelationId,
            CreatedAt = log.CreatedAt,
            CreatedBy = log.CreatedBy
        };

        private async Task EnrichAsync(ChangeHistoryLogResponse dto, CancellationToken cancellationToken)
        {
            dto.CreatedByText = await _userService.GetUserNameByIdAsync(dto.CreatedBy, cancellationToken);
        }

        public async Task<PagedListWithRelatedResponse<ChangeHistoryLogResponse>> BuildListAsync(IEnumerable<ChangeHistoryLog> logs, CancellationToken cancellationToken = default)
        {
            var list = logs.Select(Map).ToList();
            foreach (var item in list)
            {
                await EnrichAsync(item, cancellationToken);
            }

            return new PagedListWithRelatedResponse<ChangeHistoryLogResponse>
            {
                Data = list,
                PageNumber = 1,
                RowCount = list.Count,
                Related = []
            };
        }

        public async Task<EntityWithRelatedResponse<ChangeHistoryLogResponse, EmptyRelated>> BuildDetailsAsync(ChangeHistoryLog log, CancellationToken cancellationToken = default)
        {
            var dto = Map(log);
            await EnrichAsync(dto, cancellationToken);
            return new EntityWithRelatedResponse<ChangeHistoryLogResponse, EmptyRelated>
            {
                Data = dto,
                Related = new EmptyRelated()
            };
        }
    }
}
