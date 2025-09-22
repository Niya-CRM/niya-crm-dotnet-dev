using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.Common;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using System.ComponentModel.DataAnnotations;
using OXDesk.Api.Common;
using FluentValidation;
using System.Linq;

namespace OXDesk.Api.Controllers.Identity;

/// <summary>
/// Controller for managing user operations.
/// </summary>
[ApiController]
[Route("users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly IValidator<ActivateDeactivateUserRequest> _activateDeactivateUserRequestValidator;
    private readonly IUserFactory _userFactory;
    private readonly IRoleFactory _roleFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="activateDeactivateUserRequestValidator">The validator for activate/deactivate user requests.</param>
    /// <param name="userFactory">Factory for building user DTOs.</param>
    /// <param name="roleFactory">Factory for building role DTOs.</param>
    public UserController(
        IUserService userService,
        ILogger<UserController> logger,
        IValidator<ActivateDeactivateUserRequest> activateDeactivateUserRequestValidator,
        IUserFactory userFactory,
        IRoleFactory roleFactory)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activateDeactivateUserRequestValidator = activateDeactivateUserRequestValidator ?? throw new ArgumentNullException(nameof(activateDeactivateUserRequestValidator));
        _userFactory = userFactory ?? throw new ArgumentNullException(nameof(userFactory));
        _roleFactory = roleFactory ?? throw new ArgumentNullException(nameof(roleFactory));
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user with related reference data.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<UserResponse, UserDetailsRelated>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EntityWithRelatedResponse<UserResponse, UserDetailsRelated>>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Creating user with email: {Email}", request.Email);
            
            // Get the current user ID from the claims
            var currentUserId = User.FindFirst("sub")?.Value;
            int? createdBy = int.TryParse(currentUserId, out var userId) ? userId : null;
            
            var entity = await _userService.CreateUserAsync(
                request: request,
                createdBy: createdBy,
                cancellationToken: cancellationToken);

            var response = await _userFactory.BuildDetailsAsync(entity, cancellationToken);

            _logger.LogInformation("Successfully created user with ID: {UserId}", response.Data.Id);
            return CreatedAtAction(nameof(GetUserById), new { id = response.Data.Id }, response);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in user creation request: {Message}", ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User creation conflict: {Message}", ex.Message);
            return this.CreateConflictProblem(ex.Message);
        }
    }
    
    /// <summary>
    /// Gets a user by their identifier with display values.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user with display values if found, otherwise 404.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<UserResponse, UserDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID with display values: {UserId}", id);
        
        var entity = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return this.CreateNotFoundProblem($"User with ID '{id}' was not found.");
        }

        var response = await _userFactory.BuildDetailsAsync(entity, cancellationToken);
        return Ok(response);
    }
    
    /// <summary>
    /// Gets all users along with related reference data.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Users list wrapper containing data and related options.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedListWithRelatedResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all users");
        var entities = await _userService.GetAllUsersAsync(cancellationToken);
        var response = await _userFactory.BuildListAsync(entities, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Activates a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The activation request containing the reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user with related reference data.</returns>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<UserResponse, UserDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<UserResponse, UserDetailsRelated>>> ActivateUser(int id, [FromBody] ActivateDeactivateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ChangeUserActivationStatus(id, request, true, cancellationToken);
    }

    /// <summary>
    /// Deactivates a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The deactivation request containing the reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user with related reference data.</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(EntityWithRelatedResponse<UserResponse, UserDetailsRelated>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntityWithRelatedResponse<UserResponse, UserDetailsRelated>>> DeactivateUser(int id, [FromBody] ActivateDeactivateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ChangeUserActivationStatus(id, request, false, cancellationToken);
    }

    /// <summary>
    /// Private helper to change user activation status with validation and logging.
    /// </summary>
    private async Task<ActionResult<EntityWithRelatedResponse<UserResponse, UserDetailsRelated>>> ChangeUserActivationStatus(int id, ActivateDeactivateUserRequest request, bool activate, CancellationToken cancellationToken = default)
    {
        var validationResult = await _activateDeactivateUserRequestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.CreateBadRequestProblem(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        // Deny operation if targeting the default system user
        if (id == CommonConstant.DEFAULT_SYSTEM_USER)
        {
            return this.CreateBadRequestProblem("Operation is not allowed on the system user.");
        }

        // Deny operation if targeting the current authenticated user
        int currentUserId = _userService.GetCurrentUserId();
        if (id == currentUserId)
            return this.CreateBadRequestProblem("You cannot activate or deactivate your own account.");

        try
        {
            string action = activate ? UserConstant.ActivationAction.Activate : UserConstant.ActivationAction.Deactivate;
            string actionVerb = activate ? "Activating" : "Deactivating";
            _logger.LogInformation("{ActionVerb} user: {UserId}", actionVerb, id);

            var entity = await _userService.ChangeUserActivationStatusAsync(id, action, request.Reason, cancellationToken: cancellationToken);
            var response = await _userFactory.BuildDetailsAsync(entity, cancellationToken);

            string completedAction = activate ? "activated" : "deactivated";
            _logger.LogInformation("Successfully {CompletedAction} user: {UserId}", completedAction, id);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            string action = activate ? "activation" : "deactivation";
            _logger.LogWarning(ex, "Error while performing {Action} on user: {UserId}", action, id);
            return this.CreateNotFoundProblem(ex.Message);
        }
    }

    /// <summary>
    /// Gets the roles assigned to a user.
    /// </summary>
    [HttpGet("{id:guid}/roles")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupRead)]
    [ProducesResponseType(typeof(PagedListWithRelatedResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRoles(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _userService.GetUserRolesAsync(id, cancellationToken);
            var response = await _roleFactory.BuildListAsync(roles, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return this.CreateNotFoundProblem(ex.Message);
        }
    }

    /// <summary>
    /// Assigns a role to a user. Succeeds even if already assigned.
    /// </summary>
    [HttpPost("{id:guid}/roles")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(PagedListWithRelatedResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddUserRole(int id, [FromBody] AssignUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null || request.RoleId == 0)
        {
            return this.CreateBadRequestProblem("RoleId is required.");
        }

        try
        {
            var roles = await _userService.AddRoleToUserAsync(id, request.RoleId, cancellationToken: cancellationToken);
            var response = await _roleFactory.BuildListAsync(roles, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            // User or Role not found
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return this.CreateNotFoundProblem(ex.Message);
            return this.CreateBadRequestProblem(ex.Message);
        }
    }

    /// <summary>
    /// Removes a role from a user. Succeeds even if already removed.
    /// </summary>
    [HttpDelete("{id:int}/roles/{roleId:int}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(PagedListWithRelatedResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUserRole(int id, int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _userService.RemoveRoleFromUserAsync(id, roleId, cancellationToken: cancellationToken);
            var response = await _roleFactory.BuildListAsync(roles, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return this.CreateNotFoundProblem(ex.Message);
        }
    }
}
