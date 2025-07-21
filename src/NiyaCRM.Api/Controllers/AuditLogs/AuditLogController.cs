using Microsoft.AspNetCore.Mvc;
using NiyaCRM.Core.AuditLogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NiyaCRM.Core.Common;

namespace NiyaCRM.Api.Controllers
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
            [FromQuery] string? module = null,
            [FromQuery] string? mappedId = null,
            [FromQuery] string? createdBy = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT,
            [FromQuery] int pageSize = CommonConstant.PAGE_SIZE_DEFAULT,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var logs = await _auditLogService.GetAuditLogsAsync(module, mappedId, createdBy, startDate, endDate, pageNumber, pageSize, cancellationToken);
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
