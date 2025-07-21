using System;
using System.Threading;
using System.Threading.Tasks;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Core.AuditLogs;

namespace NiyaCRM.Core
{
    /// <summary>
    /// Defines the Unit of Work contract for coordinating changes across repositories.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        ITenantRepository Tenants { get; }
        IAuditLogRepository AuditLogs { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
