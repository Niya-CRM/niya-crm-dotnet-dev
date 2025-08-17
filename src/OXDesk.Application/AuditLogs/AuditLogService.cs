using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.AuditLogs.DTOs;
using Microsoft.AspNetCore.Identity;
using OXDesk.Core.Identity;
using System.Linq;

namespace OXDesk.Application.AuditLogs
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogService(IAuditLogRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        /// <inheritdoc/>
        public async Task<AuditLog> CreateAuditLogAsync(string objectKey, string @event, string objectItemId, string ip, string data, Guid createdBy, CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLog(
                Guid.CreateVersion7(),
                objectKey,
                @event,
                objectItemId,
                ip,
                data,
                createdBy
            );
            return await _repository.AddAsync(auditLog, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<AuditLog?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        private static string BuildUserDisplayName(ApplicationUser user)
        {
            var first = user.FirstName?.Trim();
            var last = user.LastName?.Trim();
            var full = string.Join(" ", new[] { first, last }.Where(s => !string.IsNullOrEmpty(s)));
            if (!string.IsNullOrEmpty(full)) return full;
            return user.Email ?? user.UserName ?? user.Id.ToString();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            AuditLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var logs = await _repository.GetAuditLogsAsync(
                query.ObjectKey,
                query.ObjectItemId,
                query.CreatedBy,
                query.StartDate,
                query.EndDate,
                query.PageNumber,
                query.PageSize,
                cancellationToken);
            // Enrich CreatedByText similar to user list enrichment
            var list = logs as IList<AuditLog> ?? logs.ToList();
            var distinctIds = new HashSet<Guid>(list.Select(l => l.CreatedBy));
            var nameMap = new Dictionary<Guid, string?>();
            foreach (var uid in distinctIds)
            {
                var user = await _userManager.FindByIdAsync(uid.ToString());
                nameMap[uid] = user == null ? uid.ToString() : BuildUserDisplayName(user);
            }
            foreach (var log in list)
            {
                log.CreatedByText = nameMap.TryGetValue(log.CreatedBy, out var name) ? name : null;
            }
            return list;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
        {
            var logs = await _repository.GetAllAsync(pageNumber, pageSize, cancellationToken);
            var list = logs as IList<AuditLog> ?? logs.ToList();
            var distinctIds = new HashSet<Guid>(list.Select(l => l.CreatedBy));
            var nameMap = new Dictionary<Guid, string?>();
            foreach (var uid in distinctIds)
            {
                var user = await _userManager.FindByIdAsync(uid.ToString());
                nameMap[uid] = user == null ? uid.ToString() : BuildUserDisplayName(user);
            }
            foreach (var log in list)
            {
                log.CreatedByText = nameMap.TryGetValue(log.CreatedBy, out var name) ? name : null;
            }
            return list;
        }
    }
}
