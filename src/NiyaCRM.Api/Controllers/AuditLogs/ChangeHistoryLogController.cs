using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NiyaCRM.Api.Common;
using NiyaCRM.Core.AuditLogs.ChangeHistory;
using NiyaCRM.Core.AuditLogs.ChangeHistory.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NiyaCRM.Api.Controllers.AuditLogs
{
    /// <summary>
    /// Controller for managing change history logs.
    /// </summary>
    [ApiController]
    [Route("audit-logs/change-history-logs")]
    public class ChangeHistoryLogController : ControllerBase
    {
        private readonly IChangeHistoryLogService _changeHistoryLogService;
        private readonly ILogger<ChangeHistoryLogController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeHistoryLogController"/> class.
        /// </summary>
        /// <param name="changeHistoryLogService">The change history log service.</param>
        /// <param name="logger">The logger.</param>
        public ChangeHistoryLogController(
            IChangeHistoryLogService changeHistoryLogService,
            ILogger<ChangeHistoryLogController> logger)
        {
            _changeHistoryLogService = changeHistoryLogService;
            _logger = logger;
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
        [ProducesResponseType(typeof(IEnumerable<ChangeHistoryLogResponseWithDisplay>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetChangeHistoryLogs(
            [FromQuery] ChangeHistoryLogQueryDto query,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate required parameters
                if (string.IsNullOrEmpty(query.ObjectKey))
                {
                    _logger.LogWarning("Missing required parameter: ObjectKey");
                    return this.CreateBadRequestProblem("ObjectKey is required");
                }

                if (query.ObjectItemId == Guid.Empty)
                {
                    _logger.LogWarning("Missing required parameter: ObjectItemId");
                    return this.CreateBadRequestProblem("ObjectItemId is required");
                }
                
                _logger.LogDebug("Retrieving change history logs with filters: {Query}", query);
                
                var logs = await _changeHistoryLogService.GetChangeHistoryLogsAsync(
                    query,
                    cancellationToken);
                
                return Ok(logs);
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
