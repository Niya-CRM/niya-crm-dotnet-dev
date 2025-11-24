using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OXDesk.Shared.Extensions.Http;
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
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly IRoleFactory _roleFactory;
        private readonly IUserFactory _userFactory;
        private readonly ILogger<PermissionsController> _logger;
        private readonly IValidator<CreatePermissionRequest> _createValidator;
        private readonly IValidator<UpdatePermissionRequest> _updateValidator;
        private readonly IPermissionFactory _permissionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsController"/> class.
        /// </summary>
        /// <param name="permissionService">The permission service.</param>
        /// <param name="roleService">The role service.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="roleFactory">Factory for building role responses.</param>
        /// <param name="userFactory">Factory for building user responses.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="createValidator">Validator for create requests.</param>
        /// <param name="updateValidator">Validator for update requests.</param>
        /// <param name="permissionFactory">Factory for building permission responses.</param>
        public PermissionsController(
            IPermissionService permissionService,
            IRoleService roleService,
            IUserService userService,
            IRoleFactory roleFactory,
            IUserFactory userFactory,
            ILogger<PermissionsController> logger,
            IValidator<CreatePermissionRequest> createValidator,
            IValidator<UpdatePermissionRequest> updateValidator,
            IPermissionFactory permissionFactory)
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _roleFactory = roleFactory ?? throw new ArgumentNullException(nameof(roleFactory));
            _userFactory = userFactory ?? throw new ArgumentNullException(nameof(userFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _permissionFactory = permissionFactory ?? throw new ArgumentNullException(nameof(permissionFactory));
        }

        [HttpGet]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<PermissionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _permissionService.GetAllPermissionsAsync(cancellationToken);
            var response = await _permissionFactory.BuildListAsync(entities, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<PermissionResponse, PermissionDetailsRelated>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _permissionService.GetPermissionByIdAsync(id, cancellationToken);
            if (entity == null) return this.CreateNotFoundProblem($"Permission with ID '{id}' was not found.");
            var response = await _permissionFactory.BuildDetailsAsync(entity, cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<PermissionResponse, PermissionDetailsRelated>), StatusCodes.Status201Created)]
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
                var entity = await _permissionService.CreatePermissionAsync(request, cancellationToken);
                var response = await _permissionFactory.BuildDetailsAsync(entity, cancellationToken);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = entity.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Create permission conflict: {Message}", ex.Message);
                return this.CreateConflictProblem(ex.Message);
            }
        }

        [HttpPatch("{id:int}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(EntityWithRelatedResponse<PermissionResponse, PermissionDetailsRelated>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdatePermissionRequest request, CancellationToken cancellationToken = default)
        {
            var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return this.CreateBadRequestProblem(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
            }
            try
            {
                var entity = await _permissionService.UpdatePermissionAsync(id, request, cancellationToken);
                var response = await _permissionFactory.BuildDetailsAsync(entity, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return this.CreateNotFoundProblem(ex.Message);
                return this.CreateConflictProblem(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status405MethodNotAllowed)]
        public IActionResult DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return this.CreateMethodNotAllowedProblem("Deleting permissions is not allowed.");
        }

        [HttpGet("{id:int}/roles")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissionRolesAsync(int id, CancellationToken cancellationToken = default)
        {
            // Ensure permission exists
            var permission = await _permissionService.GetPermissionByIdAsync(id, cancellationToken);
            if (permission is null)
            {
                return this.CreateNotFoundProblem($"Permission with ID '{id}' was not found.");
            }

            // Get role names linked to this permission
            var roleNames = await _permissionService.GetPermissionRolesAsync(id, cancellationToken);

            // Fetch roles and filter by names (case-insensitive)
            var allRoles = await _roleService.GetAllRolesAsync(cancellationToken);
            var set = roleNames.Select(n => n.Trim())
                               .Where(n => !string.IsNullOrEmpty(n))
                               .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var roles = allRoles.Where(r => (!string.IsNullOrEmpty(r.Name) && set.Contains(r.Name)) || (!string.IsNullOrEmpty(r.NormalizedName) && set.Contains(r.NormalizedName)));

            var response = await _roleFactory.BuildListAsync(roles, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id:int}/users")]
        [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
        [ProducesResponseType(typeof(PagedListWithRelatedResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissionUsersAsync(int id, CancellationToken cancellationToken = default)
        {
            // Ensure permission exists
            var permission = await _permissionService.GetPermissionByIdAsync(id, cancellationToken);
            if (permission is null)
            {
                return this.CreateNotFoundProblem($"Permission with ID '{id}' was not found.");
            }

            // Roles that include this permission
            var roleNames = await _permissionService.GetPermissionRolesAsync(id, cancellationToken);

            // Get matching role entities
            var allRoles = await _roleService.GetAllRolesAsync(cancellationToken);
            var set = roleNames.Select(n => n.Trim())
                               .Where(n => !string.IsNullOrEmpty(n))
                               .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var roles = allRoles.Where(r => (!string.IsNullOrEmpty(r.Name) && set.Contains(r.Name)) || (!string.IsNullOrEmpty(r.NormalizedName) && set.Contains(r.NormalizedName))).ToList();

            // Accumulate users from all roles, deduplicate by user Id
            var users = new List<ApplicationUser>();
            var seen = new HashSet<int>();
            foreach (var role in roles)
            {
                var roleUsers = await _userService.GetUsersByRoleIdAsync(role.Id, cancellationToken);
                foreach (var u in roleUsers)
                {
                    if (seen.Add(u.Id)) users.Add(u);
                }
            }

            var response = await _userFactory.BuildListAsync(users, cancellationToken);
            return Ok(response);
        }
    }
}

