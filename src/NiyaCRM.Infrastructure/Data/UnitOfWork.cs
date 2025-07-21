using System;
using System.Threading;
using System.Threading.Tasks;
using NiyaCRM.Core;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Core.AuditLogs;

namespace NiyaCRM.Infrastructure.Data
{
    /// <summary>
    /// Implements the Unit of Work pattern for coordinating repository changes.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        public ITenantRepository Tenants { get; }
        public IAuditLogRepository AuditLogs { get; }

        public UnitOfWork(
            ApplicationDbContext dbContext,
            ITenantRepository tenantRepository,
            IAuditLogRepository auditLogRepository)
        {
            _dbContext = dbContext;
            Tenants = tenantRepository;
            AuditLogs = auditLogRepository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
