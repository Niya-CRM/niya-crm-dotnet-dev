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

            return await q
                .OrderByDescending(c => c.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChangeHistoryLog> AddAsync(ChangeHistoryLog changeHistoryLog, CancellationToken cancellationToken = default)
        {
            if (changeHistoryLog.CreatedAt == default)
                changeHistoryLog.CreatedAt = DateTime.UtcNow;

            await _dbSet.AddAsync(changeHistoryLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return changeHistoryLog;
        }
    }
}

