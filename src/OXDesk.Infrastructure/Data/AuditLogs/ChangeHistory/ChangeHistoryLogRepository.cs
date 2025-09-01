using Microsoft.EntityFrameworkCore;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
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

        public async Task<ChangeHistoryLog?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.Id == id && c.TenantId == tenantId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<ChangeHistoryLog>> GetChangeHistoryLogsAsync(
            Guid tenantId,
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
            var query = _dbSet.AsNoTracking()
                .Where(c => c.TenantId == tenantId)
                .AsQueryable();

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

            // Left join to users and project raw data; build display name after materialization for null-safety
            var joined = from c in query
                         join u in _dbContext.Users.AsNoTracking() on c.CreatedBy equals u.Id into gj
                         from u in gj.DefaultIfEmpty()
                         orderby c.CreatedAt descending
                         select new { c, u };

            var rows = await joined
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
            Guid tenantId,
            int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
            int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.TenantId == tenantId)
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
