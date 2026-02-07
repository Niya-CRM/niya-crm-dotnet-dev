using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.AuditLogs.DTOs;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Common.Response;
using OXDesk.Core.Identity;

namespace OXDesk.Api.Factories.AuditLogs
{
    /// <summary>
    /// Builds AuditLog response DTOs and wraps them with related data.
    /// </summary>
    public sealed class AuditLogFactory : IAuditLogFactory
    {
        private readonly IUserService _userService;

        public AuditLogFactory(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private static AuditLogResponse Map(AuditLog log) => new()
        {
            Id = log.Id,
            Event = log.Event,
            ObjectId = log.ObjectId,
            ObjectItemIdUuid = log.ObjectItemIdUuid,
            ObjectItemIdInt = log.ObjectItemIdInt,
            IP = log.IP,
            Data = log.Data,
            TraceId = log.TraceId,
            CreatedAt = log.CreatedAt,
            CreatedBy = log.CreatedBy
        };

        private async Task EnrichAsync(AuditLogResponse dto, CancellationToken cancellationToken)
        {
            dto.CreatedByText = await _userService.GetUserNameByIdAsync(dto.CreatedBy, cancellationToken);
        }

        public async Task<PagedListWithRelatedResponse<AuditLogResponse>> BuildListAsync(IEnumerable<AuditLog> logs, CancellationToken cancellationToken = default)
        {
            var list = logs.Select(Map).ToList();
            foreach (var item in list)
            {
                await EnrichAsync(item, cancellationToken);
            }

            return new PagedListWithRelatedResponse<AuditLogResponse>
            {
                Data = list,
                PageNumber = 1,
                RowCount = list.Count,
                Related = []
            };
        }

        public async Task<EntityWithRelatedResponse<AuditLogResponse, EmptyRelated>> BuildDetailsAsync(AuditLog log, CancellationToken cancellationToken = default)
        {
            var dto = Map(log);
            await EnrichAsync(dto, cancellationToken);

            return new EntityWithRelatedResponse<AuditLogResponse, EmptyRelated>
            {
                Data = dto,
                Related = new EmptyRelated()
            };
        }
    }
}
