using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Api.Common;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Cache;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Api.Controllers.Identity
{
    [ApiController]
    [Route("roles")]
    [Authorize]
    public sealed class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IAuditLogService _auditLogService;
        private readonly IChangeHistoryLogService _changeHistoryLogService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<RolesController> _logger;
        private readonly IValidator<CreateRoleRequest> _createValidator;
        private readonly IValidator<UpdateRoleRequest> _updateValidator;

        public RolesController(
            IRoleService roleService,
            IAuditLogService auditLogService,
            IChangeHistoryLogService changeHistoryLogService,
            ICacheService cacheService,
            ILogger<RolesController> logger,
            IValidator<CreateRoleRequest> createValidator,
            IValidator<UpdateRoleRequest> updateValidator)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _changeHistoryLogService = changeHistoryLogService ?? throw new ArgumentNullException(nameof(changeHistoryLogService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        [HttpGet]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(RoleResponse[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = CommonConstant.CacheKeys.RolesList;
            var cached = await _cacheService.GetAsync<RoleResponse[]>(cacheKey);
            if (cached != null)
            {
                return Ok(cached);
            }

            var items = (await _roleService.GetAllRolesAsync(cancellationToken)).OrderBy(r => r.Name).ToArray();
            await _cacheService.SetAsync(cacheKey, items);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(RoleDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var details = await _roleService.GetRoleDetailsByIdAsync(id, cancellationToken);
            if (details == null) return this.CreateNotFoundProblem($"Role with ID '{id}' was not found.");
            return Ok(details);
        }

        [HttpPost]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateRoleRequest request, CancellationToken cancellationToken = default)
        {
            var validation = await _createValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return this.CreateBadRequestProblem(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
            }

            try
            {
                var response = await _roleService.CreateRoleAsync(request, createdBy: GetCurrentUserId(), cancellationToken: cancellationToken);
                await _cacheService.RemoveAsync(CommonConstant.CacheKeys.RolesList);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Create role conflict: {Message}", ex.Message);
                return this.CreateConflictProblem(ex.Message);
            }
        }

        [HttpPatch("{id:guid}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken = default)
        {
            var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return this.CreateBadRequestProblem(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
            }

            try
            {
                var response = await _roleService.UpdateRoleAsync(id, request, updatedBy: GetCurrentUserId(), cancellationToken: cancellationToken);
                await _cacheService.RemoveAsync(CommonConstant.CacheKeys.RolesList);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // Could be 404 or conflict, but RoleService throws InvalidOperationException for both cases; returning NotFound for not present
                if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return this.CreateNotFoundProblem(ex.Message);
                return this.CreateConflictProblem(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var deleted = await _roleService.DeleteRoleAsync(id, deletedBy: GetCurrentUserId(), cancellationToken: cancellationToken);
                if (!deleted) return this.CreateNotFoundProblem($"Role with ID '{id}' was not found.");

                await _cacheService.RemoveAsync(CommonConstant.CacheKeys.RolesList);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return this.CreateConflictProblem(ex.Message);
            }
        }

        [HttpGet("{id:guid}/permissions")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var perms = await _roleService.GetRolePermissionsAsync(id, cancellationToken);
                return Ok(perms);
            }
            catch (InvalidOperationException ex)
            {
                return this.CreateNotFoundProblem(ex.Message);
            }
        }

        [HttpPut("{id:guid}/permissions")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPermissionsAsync(Guid id, [FromBody] UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default)
        {
            var updated = await _roleService.SetRolePermissionsAsync(id, request.Permissions, updatedBy: GetCurrentUserId(), cancellationToken: cancellationToken);
            return Ok(updated);
        }
    }
}
