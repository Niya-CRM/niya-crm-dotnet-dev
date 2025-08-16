using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using System.ComponentModel.DataAnnotations;
using OXDesk.Api.Common;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="logger">The logger.</param>
    public UserController(
        IUserService userService,
        ILogger<UserController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        catch (ValidationException ex)
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
    [ProducesResponseType(typeof(UserResponseWithDisplay), StatusCodes.Status200OK)]
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
}
