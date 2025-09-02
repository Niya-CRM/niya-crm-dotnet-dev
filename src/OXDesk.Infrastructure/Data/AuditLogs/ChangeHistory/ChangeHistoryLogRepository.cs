using Microsoft.EntityFrameworkCore;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OXDesk.Infrastructure.Data.AuditLogs.ChangeHistory
{
    public class ChangeHistoryLogRepository : IChangeHistoryLogRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<ChangeHistoryLog> _dbSet;

        public ChangeHistoryLogRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<ChangeHistoryLog>();
        }

        public async Task<ChangeHistoryLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ChangeHistoryLog>> GetChangeHistoryLogsAsync(
            ChangeHistoryLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var q = _dbSet.AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.ObjectKey))
                q = q.Where(c => c.ObjectKey == query.ObjectKey);
            if (query.ObjectItemId != Guid.Empty)
                q = q.Where(c => c.ObjectItemId == query.ObjectItemId);
            if (!string.IsNullOrEmpty(query.FieldName))
                q = q.Where(c => c.FieldName == query.FieldName);
            if (query.CreatedBy.HasValue)
                q = q.Where(c => c.CreatedBy == query.CreatedBy.Value);
            if (query.StartDate.HasValue)
                q = q.Where(c => c.CreatedAt >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                q = q.Where(c => c.CreatedAt <= query.EndDate.Value);

            // Left join to users and project raw data; build display name after materialization for null-safety
            var joined = from c in q
                         join u in _dbContext.Users.AsNoTracking() on c.CreatedBy equals u.Id into gj
                         from u in gj.DefaultIfEmpty()
                         orderby c.CreatedAt descending
                         select new { c, u };

            var rows = await joined
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return rows.Select(x => new ChangeHistoryLog
            {
                Id = x.c.Id,
                ObjectKey = x.c.ObjectKey,
                ObjectItemId = x.c.ObjectItemId,
                FieldName = x.c.FieldName,
                OldValue = x.c.OldValue,
                NewValue = x.c.NewValue,
                CreatedAt = x.c.CreatedAt,
                CreatedBy = x.c.CreatedBy,
                TenantId = x.c.TenantId,
                CreatedByText = x.u == null
                    ? x.c.CreatedBy.ToString()
                    : (string.Join(" ", new[] { x.u.FirstName, x.u.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))) is string full && !string.IsNullOrWhiteSpace(full)
                        ? full
                        : (x.u.Email ?? x.u.UserName ?? x.c.CreatedBy.ToString()))
            });
        }

        public async Task<IEnumerable<ChangeHistoryLog>> GetAllAsync(
            int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
            int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChangeHistoryLog> AddAsync(ChangeHistoryLog changeHistoryLog, CancellationToken cancellationToken = default)
        {
            // Ensure CreatedAt is set to UTC now if not already set
            if (changeHistoryLog.CreatedAt == default)
                changeHistoryLog.CreatedAt = DateTime.UtcNow;

            await _dbSet.AddAsync(changeHistoryLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return changeHistoryLog;
        }
    }
}

