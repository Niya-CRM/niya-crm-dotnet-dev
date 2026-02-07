using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Shared.Extensions.Http;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using OXDesk.Core.Common;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.DynamicObjects;
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
        private readonly IDynamicObjectService _dynamicObjectService;
        private readonly ILogger<ChangeHistoryLogController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeHistoryLogController"/> class.
        /// </summary>
        /// <param name="changeHistoryLogService">The change history log service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="changeHistoryLogFactory">The change history log factory.</param>
        /// <param name="dynamicObjectService">The dynamic object service.</param>
        public ChangeHistoryLogController(
            IChangeHistoryLogService changeHistoryLogService,
            ILogger<ChangeHistoryLogController> logger,
            IChangeHistoryLogFactory changeHistoryLogFactory,
            IDynamicObjectService dynamicObjectService)
        {
            _changeHistoryLogService = changeHistoryLogService;
            _logger = logger;
            _changeHistoryLogFactory = changeHistoryLogFactory;
            _dynamicObjectService = dynamicObjectService;
        }

        /// <summary>
        /// Gets change history logs with optional filtering and pagination.
        /// </summary>
        /// <param name="objectKey">The object key used to resolve the dynamic object identifier.</param>
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
            [FromQuery] string objectKey,
            [FromQuery] ChangeHistoryLogQueryDto query,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate required parameters
                if (string.IsNullOrWhiteSpace(objectKey))
                {
                    _logger.LogWarning("Missing required parameter: objectKey");
                    return this.CreateBadRequestProblem("objectKey is required");
                }

                var objectId = await _dynamicObjectService.GetDynamicObjectIdAsync(objectKey, cancellationToken);
                query.ObjectId = objectId;
                
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
