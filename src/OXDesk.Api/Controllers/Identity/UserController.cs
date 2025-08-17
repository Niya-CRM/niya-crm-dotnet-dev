using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.Common;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="activateDeactivateUserRequestValidator">The validator for activate/deactivate user requests.</param>
    public UserController(
        IUserService userService,
        ILogger<UserController> logger,
        IValidator<ActivateDeactivateUserRequest> activateDeactivateUserRequestValidator)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activateDeactivateUserRequestValidator = activateDeactivateUserRequestValidator ?? throw new ArgumentNullException(nameof(activateDeactivateUserRequestValidator));
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
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
            Guid? createdBy = currentUserId != null ? Guid.Parse(currentUserId) : null;
            
            var user = await _userService.CreateUserAsync(
                request: request,
                createdBy: createdBy,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully created user with ID: {UserId}", user.Id);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
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
    [ProducesResponseType(typeof(UserDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID with display values: {UserId}", id);
        
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return this.CreateNotFoundProblem($"User with ID '{id}' was not found.");
        }

        return Ok(user);
    }
    
    /// <summary>
    /// Gets all users along with related reference data.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Users list wrapper containing data and related options.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(UsersListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all users");
        var response = await _userService.GetAllUsersAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Activates a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The activation request containing the reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user.</returns>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> ActivateUser(Guid id, [FromBody] ActivateDeactivateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ChangeUserActivationStatus(id, request, true, cancellationToken);
    }

    /// <summary>
    /// Deactivates a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The deactivation request containing the reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user.</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> DeactivateUser(Guid id, [FromBody] ActivateDeactivateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ChangeUserActivationStatus(id, request, false, cancellationToken);
    }

    /// <summary>
    /// Private helper to change user activation status with validation and logging.
    /// </summary>
    private async Task<ActionResult<UserResponse>> ChangeUserActivationStatus(Guid id, ActivateDeactivateUserRequest request, bool activate, CancellationToken cancellationToken = default)
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
        Guid currentUserId = _userService.GetCurrentUserId();
        if (id == currentUserId)
            return this.CreateBadRequestProblem("You cannot activate or deactivate your own account.");

        try
        {
            string action = activate ? UserConstant.ActivationAction.Activate : UserConstant.ActivationAction.Deactivate;
            string actionVerb = activate ? "Activating" : "Deactivating";
            _logger.LogInformation("{ActionVerb} user: {UserId}", actionVerb, id);

            var user = await _userService.ChangeUserActivationStatusAsync(id, action, request.Reason, cancellationToken: cancellationToken);

            string completedAction = activate ? "activated" : "deactivated";
            _logger.LogInformation("Successfully {CompletedAction} user: {UserId}", completedAction, id);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            string action = activate ? "activation" : "deactivation";
            _logger.LogWarning(ex, "Error while performing {Action} on user: {UserId}", action, id);
            return this.CreateNotFoundProblem(ex.Message);
        }
    }
}
