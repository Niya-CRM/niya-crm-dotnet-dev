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

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="dbContext">The application database context.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="auditLogRepository">The audit log repository.</param>
        public UnitOfWork(
            ApplicationDbContext dbContext,
            ITenantRepository tenantRepository,
            IAuditLogRepository auditLogRepository)
        {
            _dbContext = dbContext;
            Tenants = tenantRepository;
            AuditLogs = auditLogRepository;
        }

        /// <summary>
        /// Saves all changes made in this context to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private bool _disposed = false;

        /// <summary>
        /// Releases the unmanaged resources used by the UnitOfWork and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer for the UnitOfWork class.
        /// </summary>
        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
