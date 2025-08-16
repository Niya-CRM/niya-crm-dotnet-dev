using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.AuditLogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.AuditLogs.DTOs;
using OXDesk.Api.Common;

namespace OXDesk.Api.Controllers.AuditLogs
{
    [ApiController]
    [Route("audit-logs")]
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
        query,
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
                return this.CreateBadRequestProblem("Invalid model state");
            }
            var log = await _auditLogService.GetAuditLogByIdAsync(id, cancellationToken);
            if (log == null)
                return this.CreateNotFoundProblem($"Audit log with ID '{id}' was not found.");
            return Ok(log);
        }
    }
}
