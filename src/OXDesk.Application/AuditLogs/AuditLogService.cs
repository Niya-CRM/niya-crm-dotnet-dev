using Microsoft.AspNetCore.Http;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.AuditLogs.DTOs;
using System.Linq;
using OXDesk.Application.Common;

namespace OXDesk.Application.AuditLogs
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantContextService _tenantContextService;

        public AuditLogService(
            IAuditLogRepository repository, 
            IHttpContextAccessor httpContextAccessor,
            ITenantContextService tenantContextService)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
            _tenantContextService = tenantContextService;
        }

        /// <inheritdoc/>
        public async Task<AuditLog> CreateAuditLogAsync(string objectKey, string @event, string objectItemId, string ip, string data, Guid createdBy, CancellationToken cancellationToken = default)
        {
            // Get tenant ID from the tenant context service
            Guid tenantId = _tenantContextService.GetCurrentTenantId();
            
            var auditLog = new AuditLog(
                Guid.CreateVersion7(),
                tenantId,
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
            // Get tenant ID from the tenant context service
            Guid tenantId = _tenantContextService.GetCurrentTenantId();
            return await _repository.GetByIdAsync(id, tenantId, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            AuditLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            // Get tenant ID from the tenant context service
            Guid tenantId = _tenantContextService.GetCurrentTenantId();
            
            var logs = await _repository.GetAuditLogsAsync(
                tenantId,
                query.ObjectKey,
                query.ObjectItemId,
                query.CreatedBy,
                query.StartDate,
                query.EndDate,
                query.PageNumber,
                query.PageSize,
                cancellationToken);
            // Repository populates CreatedByText via join with users
            return logs;
        }
    }
}
