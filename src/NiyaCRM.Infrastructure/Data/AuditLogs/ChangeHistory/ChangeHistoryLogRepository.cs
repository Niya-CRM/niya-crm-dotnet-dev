using Microsoft.EntityFrameworkCore;
using NiyaCRM.Core.AuditLogs.ChangeHistory;
using NiyaCRM.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NiyaCRM.Infrastructure.Data.AuditLogs.ChangeHistory
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
            return await _dbSet.FindAsync([id], cancellationToken);
        }

        public async Task<IEnumerable<ChangeHistoryLog>> GetChangeHistoryLogsAsync(
            string? objectKey = null,
            Guid? objectItemId = null,
            string? fieldName = null,
            Guid? createdBy = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
            int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(objectKey))
                query = query.Where(c => c.ObjectKey == objectKey);
            if (objectItemId.HasValue)
                query = query.Where(c => c.ObjectItemId == objectItemId.Value);
            if (!string.IsNullOrEmpty(fieldName))
                query = query.Where(c => c.FieldName == fieldName);
            if (createdBy.HasValue)
                query = query.Where(c => c.CreatedBy == createdBy.Value);
            if (startDate.HasValue)
                query = query.Where(c => c.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(c => c.CreatedAt <= endDate.Value);

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ChangeHistoryLog>> GetAllAsync(
            int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
            int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChangeHistoryLog> AddAsync(ChangeHistoryLog changeHistoryLog, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(changeHistoryLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return changeHistoryLog;
        }
    }
}
