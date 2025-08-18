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
using OXDesk.Core.ValueLists;
using OXDesk.Core.ValueLists.DTOs;
using OXDesk.Application.Common.Helpers;

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
    private readonly IValueListService _valueListService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="activateDeactivateUserRequestValidator">The validator for activate/deactivate user requests.</param>
    /// <param name="valueListService">The value list service for lookups.</param>
    public UserController(
        IUserService userService,
        ILogger<UserController> logger,
        IValidator<ActivateDeactivateUserRequest> activateDeactivateUserRequestValidator,
        IValueListService valueListService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activateDeactivateUserRequestValidator = activateDeactivateUserRequestValidator ?? throw new ArgumentNullException(nameof(activateDeactivateUserRequestValidator));
        _valueListService = valueListService ?? throw new ArgumentNullException(nameof(valueListService));
    }

    private static UserResponse MapToUserResponse(ApplicationUser user) => new UserResponse
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        UserName = user.UserName ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Location = user.Location,
        TimeZone = user.TimeZone,
        CountryCode = user.CountryCode,
        PhoneNumber = user.PhoneNumber,
        Profile = user.Profile,
        IsActive = user.IsActive == "Y",
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        CreatedBy = user.CreatedBy,
        UpdatedBy = user.UpdatedBy
    };

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
            
            var entity = await _userService.CreateUserAsync(
                request: request,
                createdBy: createdBy,
                cancellationToken: cancellationToken);

            var dto = MapToUserResponse(entity);
            dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);

            _logger.LogInformation("Successfully created user with ID: {UserId}", dto.Id);
            return CreatedAtAction(nameof(GetUserById), new { id = dto.Id }, dto);
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
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID with display values: {UserId}", id);
        
        var entity = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return this.CreateNotFoundProblem($"User with ID '{id}' was not found.");
        }

        var dto = MapToUserResponse(entity);

        // Enrich with display texts using lookups
        var countriesLookup = await _valueListService.GetCountriesLookupAsync(cancellationToken);
        var profilesLookup = await _valueListService.GetUserProfilesLookupAsync(cancellationToken);

        dto.CountryCodeText = !string.IsNullOrEmpty(dto.CountryCode) && countriesLookup.TryGetValue(dto.CountryCode, out var countryItem)
            ? countryItem.ItemName
            : null;
        dto.ProfileText = !string.IsNullOrEmpty(dto.Profile) && profilesLookup.TryGetValue(dto.Profile, out var profileItem)
            ? profileItem.ItemName
            : null;

        dto.CreatedByText = await _userService.GetUserNameByIdAsync(dto.CreatedBy, cancellationToken);
        dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);

        // Build related lists
        var countries = (await _valueListService.GetCountriesAsync(cancellationToken))
            .OrderBy(c => c.ItemName)
            .ToArray();
        var profiles = (await _valueListService.GetUserProfilesAsync(cancellationToken))
            .OrderBy(p => p.ItemName)
            .ToArray();
        var timeZones = TimeZoneHelper.GetAllIanaTimeZones()
            .Select(tz => new StringOption { Value = tz.Key, Name = tz.Value })
            .ToArray();
        var statuses = _valueListService.GetStatuses().ToArray();

        var response = new EntityWithRelatedResponse<UserResponse, UserDetailsRelated>
        {
            Data = dto,
            Related = new UserDetailsRelated
            {
                Countries = countries,
                Profiles = profiles,
                TimeZones = timeZones,
                Statuses = statuses
            }
        };

        return Ok(response);
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
        var entities = await _userService.GetAllUsersAsync(cancellationToken);

        // Lookups for enrichment
        var countriesLookup = await _valueListService.GetCountriesLookupAsync(cancellationToken);
        var profilesLookup = await _valueListService.GetUserProfilesLookupAsync(cancellationToken);

        var list = entities.Select(MapToUserResponse).ToList();

        // Enrich display texts and CreatedBy/UpdatedBy names
        foreach (var u in list)
        {
            u.CountryCodeText = !string.IsNullOrEmpty(u.CountryCode) && countriesLookup.TryGetValue(u.CountryCode, out var countryItem)
                ? countryItem.ItemName
                : null;
            u.ProfileText = !string.IsNullOrEmpty(u.Profile) && profilesLookup.TryGetValue(u.Profile, out var profileItem)
                ? profileItem.ItemName
                : null;

            u.CreatedByText = await _userService.GetUserNameByIdAsync(u.CreatedBy, cancellationToken);
            u.UpdatedByText = await _userService.GetUserNameByIdAsync(u.UpdatedBy, cancellationToken);
        }

        var profiles = (await _valueListService.GetUserProfilesAsync(cancellationToken))
            .Select(i => new ValueListItemOption { Id = i.Id, ItemName = i.ItemName, ItemKey = i.ItemKey, IsActive = i.IsActive })
            .OrderBy(p => p.ItemName)
            .ToArray();
        var statuses = _valueListService.GetStatuses().ToArray();

        var response = new UsersListResponse
        {
            Data = list,
            Related = new UsersListRelated
            {
                Profiles = profiles,
                Statuses = statuses
            }
        };

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

            var entity = await _userService.ChangeUserActivationStatusAsync(id, action, request.Reason, cancellationToken: cancellationToken);
            var dto = MapToUserResponse(entity);
            dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);

            string completedAction = activate ? "activated" : "deactivated";
            _logger.LogInformation("Successfully {CompletedAction} user: {UserId}", completedAction, id);
            return Ok(dto);
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
    public async Task<IActionResult> GetUserRoles(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _userService.GetUserRolesAsync(id, cancellationToken);
            var list = roles.Select(MapToRoleResponse).ToList();
            foreach (var r in list)
            {
                r.UpdatedByText = await _userService.GetUserNameByIdAsync(r.UpdatedBy, cancellationToken);
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
    public async Task<IActionResult> AddUserRole(Guid id, [FromBody] AssignUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null || request.RoleId == Guid.Empty)
        {
            return this.CreateBadRequestProblem("RoleId is required.");
        }

        try
        {
            var roles = await _userService.AddRoleToUserAsync(id, request.RoleId, cancellationToken: cancellationToken);
            var list = roles.Select(MapToRoleResponse).ToList();
            foreach (var r in list)
            {
                r.UpdatedByText = await _userService.GetUserNameByIdAsync(r.UpdatedBy, cancellationToken);
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
    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    [Authorize(Policy = CommonConstant.PermissionNames.SysSetupWrite)]
    [ProducesResponseType(typeof(PagedListWithRelatedResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUserRole(Guid id, Guid roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _userService.RemoveRoleFromUserAsync(id, roleId, cancellationToken: cancellationToken);
            var list = roles.Select(MapToRoleResponse).ToList();
            foreach (var r in list)
            {
                r.UpdatedByText = await _userService.GetUserNameByIdAsync(r.UpdatedBy, cancellationToken);
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
        catch (InvalidOperationException ex)
        {
            return this.CreateNotFoundProblem(ex.Message);
        }
    }
}
