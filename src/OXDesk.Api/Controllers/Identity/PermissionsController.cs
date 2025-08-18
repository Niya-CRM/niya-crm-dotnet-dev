using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Api.Common;
using OXDesk.Core.Common;
using OXDesk.Core.Common.DTOs;
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
        private readonly ILogger<PermissionsController> _logger;
        private readonly IValidator<CreatePermissionRequest> _createValidator;
        private readonly IValidator<UpdatePermissionRequest> _updateValidator;
        private readonly IUserService _userService;

        public PermissionsController(
            IPermissionService permissionService,
            ILogger<PermissionsController> logger,
            IValidator<CreatePermissionRequest> createValidator,
            IValidator<UpdatePermissionRequest> updateValidator,
            IUserService userService)
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private static PermissionResponse MapToPermissionResponse(Permission p) => new PermissionResponse
        {
            Id = p.Id,
            Name = p.Name,
            NormalizedName = p.NormalizedName,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy,
            UpdatedAt = p.UpdatedAt,
            UpdatedBy = p.UpdatedBy,
        };

        private async Task EnrichPermissionAsync(PermissionResponse response, CancellationToken cancellationToken)
        {
            response.UpdatedByText = await _userService.GetUserNameByIdAsync(response.UpdatedBy, cancellationToken);
        }

        [HttpGet]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<PermissionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _permissionService.GetAllPermissionsAsync(cancellationToken);
            var list = entities.Select(MapToPermissionResponse).ToList();
            foreach (var item in list)
            {
                await EnrichPermissionAsync(item, cancellationToken);
            }
            var response = new PagedListWithRelatedResponse<PermissionResponse>
            {
                Data = list,
                PageNumber = 1,
                RowCount = list.Count,
                Related = Array.Empty<object>()
            };
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<PermissionResponse, PermissionDetailsRelated>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _permissionService.GetPermissionByIdAsync(id, cancellationToken);
            if (entity == null) return this.CreateNotFoundProblem($"Permission with ID '{id}' was not found.");
            var dto = MapToPermissionResponse(entity);
            await EnrichPermissionAsync(dto, cancellationToken);
            var roles = await _permissionService.GetPermissionRolesAsync(id, cancellationToken);
            var response = new EntityWithRelatedResponse<PermissionResponse, PermissionDetailsRelated>
            {
                Data = dto,
                Related = new PermissionDetailsRelated { Roles = roles }
            };
            return Ok(response);
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
                var entity = await _permissionService.CreatePermissionAsync(request, createdBy: this.GetCurrentUserId(), cancellationToken: cancellationToken);
                var dto = MapToPermissionResponse(entity);
                await EnrichPermissionAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = dto.Id }, dto);
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
            try
            {
                var entity = await _permissionService.UpdatePermissionAsync(id, request, updatedBy: this.GetCurrentUserId(), cancellationToken: cancellationToken);
                var dto = MapToPermissionResponse(entity);
                await EnrichPermissionAsync(dto, cancellationToken);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return this.CreateNotFoundProblem(ex.Message);
                return this.CreateConflictProblem(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status405MethodNotAllowed)]
        public IActionResult DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return this.CreateMethodNotAllowedProblem("Deleting permissions is not allowed.");
        }
    }
}

