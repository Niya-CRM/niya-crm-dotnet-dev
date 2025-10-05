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
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<AuditLog> _dbSet;

        public AuditLogRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<AuditLog>();
        }

        public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            AuditLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var q = _dbSet.AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.ObjectKey))
                q = q.Where(a => a.ObjectKey == query.ObjectKey);
            if (query.ObjectItemId.HasValue)
                q = q.Where(a => a.ObjectItemId == query.ObjectItemId.Value);
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
