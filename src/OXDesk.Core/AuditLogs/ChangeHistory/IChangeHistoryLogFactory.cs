using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Common.Response;

namespace OXDesk.Core.AuditLogs.ChangeHistory
{
    /// <summary>
    /// Factory interface for building ChangeHistoryLog DTOs and wrapping with related data.
    /// </summary>
    public interface IChangeHistoryLogFactory
    {
        Task<PagedListWithRelatedResponse<ChangeHistoryLogResponse>> BuildListAsync(IEnumerable<ChangeHistoryLog> logs, CancellationToken cancellationToken = default);

        Task<EntityWithRelatedResponse<ChangeHistoryLogResponse, EmptyRelated>> BuildDetailsAsync(ChangeHistoryLog log, CancellationToken cancellationToken = default);
    }
}
