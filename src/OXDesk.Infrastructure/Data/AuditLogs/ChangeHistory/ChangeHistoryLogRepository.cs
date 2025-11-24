using Microsoft.EntityFrameworkCore;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using OXDesk.DbContext.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OXDesk.Infrastructure.Data.AuditLogs.ChangeHistory
{
    /// <summary>
    /// Repository implementation for change history log data access operations.
    /// </summary>
    public class ChangeHistoryLogRepository : IChangeHistoryLogRepository
    {
        private readonly TenantDbContext _dbContext;
        private readonly DbSet<ChangeHistoryLog> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeHistoryLogRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The tenant database context.</param>
        public ChangeHistoryLogRepository(TenantDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<ChangeHistoryLog>();
        }

        /// <inheritdoc/>
        public async Task<ChangeHistoryLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChangeHistoryLog>> GetChangeHistoryLogsAsync(
            ChangeHistoryLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var q = _dbSet.AsNoTracking()
                .AsQueryable();

            if (query.ObjectId.HasValue)
                q = q.Where(c => c.ObjectId == query.ObjectId.Value);
            if (query.ObjectItemIdInt.HasValue)
                q = q.Where(c => c.ObjectItemIdInt == query.ObjectItemIdInt.Value);
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

        /// <inheritdoc/>
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

