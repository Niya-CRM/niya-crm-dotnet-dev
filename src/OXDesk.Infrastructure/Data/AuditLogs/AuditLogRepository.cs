using Microsoft.EntityFrameworkCore;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using OXDesk.Core.AuditLogs.DTOs;
using OXDesk.Infrastructure.Data.AuditLogs;
using OXDesk.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OXDesk.Infrastructure.Data.AuditLogs
{
    /// <summary>
    /// Repository implementation for audit log data access operations.
    /// </summary>
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly TenantDbContext _dbContext;
        private readonly DbSet<AuditLog> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The tenant database context.</param>
        public AuditLogRepository(TenantDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<AuditLog>();
        }

        /// <inheritdoc/>
        public async Task<AuditLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            AuditLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var q = _dbSet.AsNoTracking()
                .AsQueryable();

            if (query.ObjectId.HasValue)
                q = q.Where(a => a.ObjectId == query.ObjectId.Value);
            if (query.ObjectItemIdUuid.HasValue)
                q = q.Where(a => a.ObjectItemIdUuid == query.ObjectItemIdUuid.Value);
            if (query.ObjectItemIdInt.HasValue)
                q = q.Where(a => a.ObjectItemIdInt == query.ObjectItemIdInt.Value);
            if (query.CreatedBy.HasValue)
                q = q.Where(a => a.CreatedBy == query.CreatedBy.Value);
            if (query.StartDate.HasValue)
                q = q.Where(a => a.CreatedAt >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                q = q.Where(a => a.CreatedAt <= query.EndDate.Value);

            return await q
                .OrderByDescending(a => a.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            if (auditLog.CreatedAt == default)
                auditLog.CreatedAt = DateTime.UtcNow;

            await _dbSet.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return auditLog;
        }
    }
}
