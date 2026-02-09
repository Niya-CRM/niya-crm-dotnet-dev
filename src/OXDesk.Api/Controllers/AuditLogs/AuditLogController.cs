using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.AuditLogs.DTOs;
using OXDesk.Shared.Extensions.Http;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Common.Response;

namespace OXDesk.Api.Controllers.AuditLogs
{
    [ApiController]
    [Route("audit-logs")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IAuditLogFactory _auditLogFactory;
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(IAuditLogService auditLogService, IAuditLogFactory auditLogFactory, ILogger<AuditLogController> logger)
        {
            _auditLogService = auditLogService;
            _auditLogFactory = auditLogFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets all audit logs with optional filters and paging.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<AuditLogResponse>), 200)]
        public async Task<ActionResult<PagedListWithRelatedResponse<AuditLogResponse>>> GetAll(
            [FromQuery] string objectKey,
            [FromQuery] AuditLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(objectKey))
            {
                _logger.LogWarning("Missing required parameter: objectKey");
                return this.CreateBadRequestProblem("objectKey is required");
            }

            query.ObjectKey = objectKey;

            var logs = await _auditLogService.GetAuditLogsAsync(
                query,
                cancellationToken);

            var response = await _auditLogFactory.BuildListAsync(logs, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Gets a specific audit log by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<AuditLogResponse, EmptyRelated>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EntityWithRelatedResponse<AuditLogResponse, EmptyRelated>>> GetById(int id, CancellationToken cancellationToken = default)
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
