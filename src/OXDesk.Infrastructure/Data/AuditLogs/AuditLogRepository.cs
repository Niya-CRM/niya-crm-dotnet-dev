using Microsoft.EntityFrameworkCore;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
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
            return await _dbSet.FindAsync([id], cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            string? objectKey = null,
            string? objectItemId = null,
            Guid? createdBy = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
            int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(objectKey))
                query = query.Where(a => a.ObjectKey == objectKey);
            if (!string.IsNullOrEmpty(objectItemId))
                query = query.Where(a => a.ObjectItemId == objectItemId);
            if (createdBy.HasValue)
                query = query.Where(a => a.CreatedBy == createdBy.Value);
            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            // Left join to users, select raw data; build display name client-side to avoid expression tree null-propagation
            var joined = from a in query
                         join u in _dbContext.Users.AsNoTracking() on a.CreatedBy equals u.Id into gj
                         from u in gj.DefaultIfEmpty()
                         orderby a.CreatedAt descending
                         select new { a, u };

            var rows = await joined
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                CreatedByText = string.Join(" ", new[] { x.u.FirstName, x.u.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))) ?? x.a.CreatedBy.ToString()
            });
        }

        public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return auditLog;
        }
    }
}
