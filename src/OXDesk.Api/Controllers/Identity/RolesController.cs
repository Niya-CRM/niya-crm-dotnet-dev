using System;
using System.Collections.Generic;
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
        private readonly IPermissionService _permissionService;
        private readonly IUserService _userService;
        private readonly IUserFactory _userFactory;
        private readonly ILogger<RolesController> _logger;
        private readonly IValidator<CreateRoleRequest> _createValidator;
        private readonly IValidator<UpdateRoleRequest> _updateValidator;
        private readonly IRoleFactory _roleFactory;

        public RolesController(
            IRoleService roleService,
            IPermissionService permissionService,
            IUserService userService,
            IUserFactory userFactory,
            ILogger<RolesController> logger,
            IValidator<CreateRoleRequest> createValidator,
            IValidator<UpdateRoleRequest> updateValidator,
            IRoleFactory roleFactory)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userFactory = userFactory ?? throw new ArgumentNullException(nameof(userFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _roleFactory = roleFactory ?? throw new ArgumentNullException(nameof(roleFactory));
        }

        

        [HttpGet]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<RoleResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var roles = await _roleService.GetAllRolesAsync(cancellationToken);
            var response = await _roleFactory.BuildListAsync(roles, cancellationToken);
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
            var response = await _roleFactory.BuildDetailsAsync(role, cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<RoleResponse, RoleDetailsRelated>), StatusCodes.Status201Created)]
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
                var entity = await _roleService.CreateRoleAsync(request, cancellationToken);
                var response = await _roleFactory.BuildDetailsAsync(entity, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Create role conflict: {Message}", ex.Message);
                return this.CreateConflictProblem(ex.Message);
            }
        }

        [HttpPatch("{id:guid}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<RoleResponse, RoleDetailsRelated>), StatusCodes.Status200OK)]
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
                var entity = await _roleService.UpdateRoleAsync(id, request, cancellationToken);
                var response = await _roleFactory.BuildDetailsAsync(entity, cancellationToken);
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status405MethodNotAllowed)]
        public IActionResult DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return this.CreateMethodNotAllowedProblem("Deleting roles is not allowed.");
        }

        [HttpGet("{id:guid}/permissions")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<RolePermissionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Fetch claims with audit fields
                var claims = await _roleService.GetRolePermissionClaimsAsync(id, cancellationToken);
                var response = await _roleFactory.BuildPermissionsListAsync(claims, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return this.CreateNotFoundProblem(ex.Message);
            }
        }

        [HttpDelete("{id:guid}/permissions/{permissionId:int}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<RoleResponse, RoleDetailsRelated>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePermissionAsync(Guid id, int permissionId, CancellationToken cancellationToken = default)
        {
            // Ensure role exists
            var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
            if (role is null)
            {
                return this.CreateNotFoundProblem($"Role with ID '{id}' was not found.");
            }

            // Resolve permission by Id
            var permission = await _permissionService.GetPermissionByIdAsync(permissionId, cancellationToken);
            if (permission is null)
            {
                return this.CreateNotFoundProblem($"Permission with ID '{permissionId}' was not found.");
            }

            // Remove permission by name if present
            var current = await _roleService.GetRolePermissionsAsync(id, cancellationToken);
            var updated = current.Where(p => !string.Equals(p, permission.Name, StringComparison.OrdinalIgnoreCase)).ToArray();

            // Only call service if change is needed
            if (updated.Length != current.Length)
            {
                await _roleService.SetRolePermissionsAsync(id, updated, cancellationToken);
            }

            // Return updated role details with related permissions via factory
            var response = await _roleFactory.BuildDetailsAsync(role, cancellationToken);
            return Ok(response);
        }

        [HttpPut("{id:guid}/permissions")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<RoleResponse, RoleDetailsRelated>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPermissionsAsync(Guid id, [FromBody] UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _roleService.SetRolePermissionsAsync(id, request.Permissions, cancellationToken);

                var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
                if (role is null)
                {
                    return this.CreateNotFoundProblem($"Role with ID '{id}' was not found.");
                }

                var response = await _roleFactory.BuildDetailsAsync(role, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return this.CreateNotFoundProblem(ex.Message);
            }
        }

        [HttpGet("{id:guid}/users")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleUsersAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _userService.GetUsersByRoleIdAsync(id, cancellationToken);
                var response = await _userFactory.BuildListAsync(users, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // Role not found
                return this.CreateNotFoundProblem(ex.Message);
            }
        }
    }
}
