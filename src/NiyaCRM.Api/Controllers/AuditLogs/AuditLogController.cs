using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Core.AuditLogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NiyaCRM.Core.AuditLogs.DTOs;

namespace NiyaCRM.Api.Controllers.AuditLogs
{
    [ApiController]
    [Route("api/audit-logs")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Gets all audit logs with optional filters and paging.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AuditLog>), 200)]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAll(
    [FromQuery] AuditLogQueryDto query,
    CancellationToken cancellationToken = default)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    var logs = await _auditLogService.GetAuditLogsAsync(
        query.ObjectKey,
        query.ObjectItemId,
        query.CreatedBy,
        query.StartDate,
        query.EndDate,
        query.PageNumber,
        query.PageSize,
        cancellationToken);
    return Ok(logs);
}

        /// <summary>
        /// Gets a specific audit log by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuditLog), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AuditLog>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var log = await _auditLogService.GetAuditLogByIdAsync(id, cancellationToken);
            if (log == null)
                return NotFound();
            return Ok(log);
        }
    }
}
