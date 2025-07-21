using Microsoft.EntityFrameworkCore;
using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Core.Common;
using NiyaCRM.Infrastructure.Data.AuditLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NiyaCRM.Infrastructure.Data.AuditLogs
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
            string? module = null,
            string? mappedId = null,
            string? createdBy = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
            int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(module))
                query = query.Where(a => a.Module == module);
            if (!string.IsNullOrEmpty(mappedId))
                query = query.Where(a => a.MappedId == mappedId);
            if (!string.IsNullOrEmpty(createdBy))
                query = query.Where(a => a.CreatedBy == createdBy);
            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return auditLog;
        }
    }
}
