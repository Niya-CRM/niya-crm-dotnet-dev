using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.AuditLogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OXDesk.Core.AuditLogs.DTOs;
using OXDesk.Api.Common;
using OXDesk.Core.Common.DTOs;

namespace OXDesk.Api.Controllers.AuditLogs
{
    [ApiController]
    [Route("audit-logs")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IAuditLogFactory _auditLogFactory;

        public AuditLogController(IAuditLogService auditLogService, IAuditLogFactory auditLogFactory)
        {
            _auditLogService = auditLogService;
            _auditLogFactory = auditLogFactory;
        }

        /// <summary>
        /// Gets all audit logs with optional filters and paging.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<AuditLogResponse>), 200)]
        public async Task<ActionResult<PagedListWithRelatedResponse<AuditLogResponse>>> GetAll(
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

            var response = await _auditLogFactory.BuildListAsync(logs, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Gets a specific audit log by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<AuditLogResponse, AuditLogDetailsRelated>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EntityWithRelatedResponse<AuditLogResponse, AuditLogDetailsRelated>>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return this.CreateBadRequestProblem("Invalid model state");
            }
            var log = await _auditLogService.GetAuditLogByIdAsync(id, cancellationToken);
            if (log == null)
                return this.CreateNotFoundProblem($"Audit log with ID '{id}' was not found.");
            var response = await _auditLogFactory.BuildDetailsAsync(log, cancellationToken);
            return Ok(response);
        }
    }
}
