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
    [Route("permissions")]
    [Authorize]
    public sealed class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IAuditLogService _auditLogService;
        private readonly IChangeHistoryLogService _changeHistoryLogService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PermissionsController> _logger;
        private readonly IValidator<CreatePermissionRequest> _createValidator;
        private readonly IValidator<UpdatePermissionRequest> _updateValidator;

        public PermissionsController(
            IPermissionService permissionService,
            IAuditLogService auditLogService,
            IChangeHistoryLogService changeHistoryLogService,
            ICacheService cacheService,
            ILogger<PermissionsController> logger,
            IValidator<CreatePermissionRequest> createValidator,
            IValidator<UpdatePermissionRequest> updateValidator)
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _changeHistoryLogService = changeHistoryLogService ?? throw new ArgumentNullException(nameof(changeHistoryLogService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        private static PermissionResponse MapToResponse(Permission p) => new PermissionResponse
        {
            Id = p.Id,
            Name = p.Name,
            NormalizedName = p.NormalizedName,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy,
            UpdatedAt = p.UpdatedAt,
            UpdatedBy = p.UpdatedBy,
        };

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        [HttpGet]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PermissionResponse[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = CommonConstant.CacheKeys.PermissionsList;
            var cached = await _cacheService.GetAsync<PermissionResponse[]>(cacheKey);
            if (cached != null)
            {
                return Ok(cached);
            }

            var items = (await _permissionService.GetAllPermissionsAsync()).Select(MapToResponse).OrderBy(x => x.Name).ToArray();
            await _cacheService.SetAsync(cacheKey, items);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _permissionService.GetPermissionByIdAsync(id);
            if (entity == null) return this.CreateNotFoundProblem($"Permission with ID '{id}' was not found.");
            return Ok(MapToResponse(entity));
        }

        [HttpPost]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePermissionRequest request, CancellationToken cancellationToken = default)
        {
            var validation = await _createValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return this.CreateBadRequestProblem(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
            }

            try
            {
                var userId = GetCurrentUserId() ?? CommonConstant.DEFAULT_SYSTEM_USER;
                var now = DateTime.UtcNow;
                var entity = new Permission
                {
                    Id = Guid.CreateVersion7(),
                    Name = request.Name.Trim(),
                    NormalizedName = request.Name.Trim().ToUpperInvariant(),
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = userId,
                    UpdatedBy = userId
                };

                entity = await _permissionService.AddPermissionAsync(entity);

                await _auditLogService.CreateAuditLogAsync(
                    objectKey: CommonConstant.MODULE_PERMISSION,
                    @event: CommonConstant.AUDIT_LOG_EVENT_CREATE,
                    objectItemId: entity.Id.ToString(),
                    data: $"Permission created: {entity.Name}",
                    createdBy: userId,
                    ip: HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    cancellationToken: cancellationToken);

                await _cacheService.RemoveAsync(CommonConstant.CacheKeys.PermissionsList);

                var response = MapToResponse(entity);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Create permission conflict: {Message}", ex.Message);
                return this.CreateConflictProblem(ex.Message);
            }
        }

        [HttpPatch("{id:guid}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdatePermissionRequest request, CancellationToken cancellationToken = default)
        {
            var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return this.CreateBadRequestProblem(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
            }

            var entity = await _permissionService.GetPermissionByIdAsync(id);
            if (entity == null) return this.CreateNotFoundProblem($"Permission with ID '{id}' was not found.");

            var userId = GetCurrentUserId() ?? CommonConstant.DEFAULT_SYSTEM_USER;
            var now = DateTime.UtcNow;

            var oldName = entity.Name;
            var newName = request.Name.Trim();
            var newNormalized = newName.ToUpperInvariant();

            // uniqueness check
            var dup = await _permissionService.GetPermissionByNameAsync(newNormalized);
            if (dup != null && dup.Id != entity.Id)
            {
                return this.CreateConflictProblem($"A permission with the same name already exists.");
            }

            entity.Name = newName;
            entity.NormalizedName = newNormalized;
            entity.UpdatedAt = now;
            entity.UpdatedBy = userId;

            entity = await _permissionService.UpdatePermissionAsync(entity);

            if (!string.Equals(oldName, entity.Name, StringComparison.Ordinal))
            {
                await _changeHistoryLogService.CreateChangeHistoryLogAsync(
                    objectKey: CommonConstant.MODULE_PERMISSION,
                    objectItemId: entity.Id,
                    fieldName: "name",
                    oldValue: oldName,
                    newValue: entity.Name,
                    createdBy: userId,
                    cancellationToken: cancellationToken);
            }

            await _auditLogService.CreateAuditLogAsync(
                objectKey: CommonConstant.MODULE_PERMISSION,
                @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
                objectItemId: entity.Id.ToString(),
                data: $"Permission updated: {entity.Name}",
                createdBy: userId,
                ip: HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                cancellationToken: cancellationToken);

            await _cacheService.RemoveAsync(CommonConstant.CacheKeys.PermissionsList);

            return Ok(MapToResponse(entity));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId() ?? CommonConstant.DEFAULT_SYSTEM_USER;
            var deleted = await _permissionService.DeletePermissionAsync(id);
            if (!deleted)
            {
                return this.CreateNotFoundProblem($"Permission with ID '{id}' was not found.");
            }

            await _changeHistoryLogService.CreateChangeHistoryLogAsync(
                objectKey: CommonConstant.MODULE_PERMISSION,
                objectItemId: id,
                fieldName: CommonConstant.ChangeHistoryFields.Deleted,
                oldValue: "N",
                newValue: "Y",
                createdBy: userId,
                cancellationToken: cancellationToken);

            await _auditLogService.CreateAuditLogAsync(
                objectKey: CommonConstant.MODULE_PERMISSION,
                @event: CommonConstant.AUDIT_LOG_EVENT_DELETE,
                objectItemId: id.ToString(),
                data: $"Permission deleted: {id}",
                createdBy: userId,
                ip: HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                cancellationToken: cancellationToken);

            await _cacheService.RemoveAsync(CommonConstant.CacheKeys.PermissionsList);
            return NoContent();
        }
    }
}
