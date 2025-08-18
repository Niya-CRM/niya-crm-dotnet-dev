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
    [Route("roles")]
    [Authorize]
    public sealed class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;
        private readonly IValidator<CreateRoleRequest> _createValidator;
        private readonly IValidator<UpdateRoleRequest> _updateValidator;
        private readonly IUserService _userService;

        public RolesController(
            IRoleService roleService,
            ILogger<RolesController> logger,
            IValidator<CreateRoleRequest> createValidator,
            IValidator<UpdateRoleRequest> updateValidator,
            IUserService userService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        

        private static RoleResponse MapToRoleResponse(ApplicationRole role) => new RoleResponse
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            NormalizedName = role.NormalizedName ?? string.Empty,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            CreatedBy = role.CreatedBy,
            UpdatedBy = role.UpdatedBy
        };

        private async Task EnrichRoleAsync(RoleResponse role, CancellationToken cancellationToken)
        {
            role.UpdatedByText = await _userService.GetUserNameByIdAsync(role.UpdatedBy, cancellationToken);
        }

        [HttpGet]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<RoleResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var roles = await _roleService.GetAllRolesAsync(cancellationToken);
            var list = roles.Select(MapToRoleResponse).ToList();
            foreach (var r in list)
            {
                await EnrichRoleAsync(r, cancellationToken);
            }
            var response = new PagedListWithRelatedResponse<RoleResponse>
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
        [ProducesResponseType(typeof(EntityWithRelatedResponse<RoleResponse, RoleDetailsRelated>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
            if (role == null) return this.CreateNotFoundProblem($"Role with ID '{id}' was not found.");
            var dto = MapToRoleResponse(role);
            await EnrichRoleAsync(dto, cancellationToken);
            var perms = await _roleService.GetRolePermissionsAsync(id, cancellationToken);
            var response = new EntityWithRelatedResponse<RoleResponse, RoleDetailsRelated>
            {
                Data = dto,
                Related = new RoleDetailsRelated { Permissions = perms }
            };
            return Ok(response);
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
                var entity = await _roleService.CreateRoleAsync(request, createdBy: this.GetCurrentUserId(), cancellationToken: cancellationToken);
                var dto = MapToRoleResponse(entity);
                await EnrichRoleAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = dto.Id }, dto);
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
                var entity = await _roleService.UpdateRoleAsync(id, request, updatedBy: this.GetCurrentUserId(), cancellationToken: cancellationToken);
                var dto = MapToRoleResponse(entity);
                await EnrichRoleAsync(dto, cancellationToken);
                return Ok(dto);
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status405MethodNotAllowed)]
        public IActionResult DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return this.CreateMethodNotAllowedProblem("Deleting roles is not allowed.");
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
            var updated = await _roleService.SetRolePermissionsAsync(id, request.Permissions, updatedBy: this.GetCurrentUserId(), cancellationToken: cancellationToken);
            return Ok(updated);
        }
    }
}
