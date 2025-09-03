using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.AuditLogs.DTOs;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Common.Response;

namespace OXDesk.Core.AuditLogs
{
    /// <summary>
    /// Factory interface for building audit log DTOs and response wrappers, including enrichment and related data.
    /// </summary>
    public interface IAuditLogFactory
    {
        Task<PagedListWithRelatedResponse<AuditLogResponse>> BuildListAsync(IEnumerable<AuditLog> logs, CancellationToken cancellationToken = default);

        Task<EntityWithRelatedResponse<AuditLogResponse, EmptyRelated>> BuildDetailsAsync(AuditLog log, CancellationToken cancellationToken = default);
    }
}
