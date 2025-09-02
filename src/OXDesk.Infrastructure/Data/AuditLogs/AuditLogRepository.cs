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
            if (!string.IsNullOrEmpty(query.ObjectItemId))
                q = q.Where(a => a.ObjectItemId == query.ObjectItemId);
            if (query.CreatedBy.HasValue)
                q = q.Where(a => a.CreatedBy == query.CreatedBy.Value);
            if (query.StartDate.HasValue)
                q = q.Where(a => a.CreatedAt >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                q = q.Where(a => a.CreatedAt <= query.EndDate.Value);

            // Left join to users, select raw data; build display name client-side to avoid expression tree null-propagation
            var joined = from a in q
                         join u in _dbContext.Users.AsNoTracking() on a.CreatedBy equals u.Id into gj
                         from u in gj.DefaultIfEmpty()
                         orderby a.CreatedAt descending
                         select new { a, u };

            var rows = await joined
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return rows.Select(x => new AuditLog
            {
                Id = x.a.Id,
                ObjectKey = x.a.ObjectKey,
                Event = x.a.Event,
                ObjectItemId = x.a.ObjectItemId,
                IP = x.a.IP,
                Data = x.a.Data,
                CreatedAt = x.a.CreatedAt,
                CreatedBy = x.a.CreatedBy,
                TenantId = x.a.TenantId,
                CreatedByText = string.Join(" ", new[] { x.u.FirstName, x.u.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))) ?? x.a.CreatedBy.ToString()
            });
        }

        public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            // Ensure CreatedAt is set to UTC now if not already set
            if (auditLog.CreatedAt == default)
                auditLog.CreatedAt = DateTime.UtcNow;

            await _dbSet.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return auditLog;
        }
    }
}
