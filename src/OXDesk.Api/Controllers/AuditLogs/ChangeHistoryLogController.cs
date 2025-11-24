using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Shared.Extensions.Http;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using OXDesk.Core.Common;
using OXDesk.Core.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OXDesk.Api.Controllers.AuditLogs
{
    /// <summary>
    /// Controller for managing change history logs.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("audit-logs/change-history-logs")]
    public class ChangeHistoryLogController : ControllerBase
    {
        private readonly IChangeHistoryLogService _changeHistoryLogService;
        private readonly IChangeHistoryLogFactory _changeHistoryLogFactory;
        private readonly ILogger<ChangeHistoryLogController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeHistoryLogController"/> class.
        /// </summary>
        /// <param name="changeHistoryLogService">The change history log service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="changeHistoryLogFactory">The change history log factory.</param>
        public ChangeHistoryLogController(
            IChangeHistoryLogService changeHistoryLogService,
            ILogger<ChangeHistoryLogController> logger,
            IChangeHistoryLogFactory changeHistoryLogFactory)
        {
            _changeHistoryLogService = changeHistoryLogService;
            _logger = logger;
            _changeHistoryLogFactory = changeHistoryLogFactory;
        }

        /// <summary>
        /// Gets change history logs with optional filtering and pagination.
        /// </summary>
        /// <param name="query">The query parameters for filtering change history logs.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of change history logs matching the filters.</returns>
        /// <response code="200">Returns the list of change history logs.</response>
        /// <response code="400">If the query parameters are invalid.</response>
        [HttpGet]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<ChangeHistoryLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedListWithRelatedResponse<ChangeHistoryLogResponse>>> GetChangeHistoryLogs(
            [FromQuery] ChangeHistoryLogQueryDto query,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate required parameters
                if (!query.ObjectId.HasValue)
                {
                    _logger.LogWarning("Missing required parameter: ObjectId");
                    return this.CreateBadRequestProblem("ObjectId is required");
                }

                if (!query.ObjectItemIdInt.HasValue)
                {
                    _logger.LogWarning("Missing required parameter: ObjectItemIdInt");
                    return this.CreateBadRequestProblem("ObjectItemIdInt is required");
                }
                
                _logger.LogDebug("Retrieving change history logs with filters: {Query}", query);
                
                var logs = await _changeHistoryLogService.GetChangeHistoryLogsAsync(query, cancellationToken);
                var response = await _changeHistoryLogFactory.BuildListAsync(logs, cancellationToken);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid change history log query parameters: {Message}", ex.Message);
                return this.CreateBadRequestProblem(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving change history logs: {Message}", ex.Message);
                throw;
            }
        }
    }
}
