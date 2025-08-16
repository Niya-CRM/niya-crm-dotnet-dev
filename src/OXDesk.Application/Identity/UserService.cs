using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using OXDesk.Core.Cache;
using OXDesk.Core.ValueLists;
using OXDesk.Core.ValueLists.DTOs;

namespace OXDesk.Application.Identity;

/// <summary>
/// Service implementation for user management operations.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IValueListService _valueListService;
    
    // Cache key prefix for users
    private const string USER_CACHE_KEY_PREFIX = "user_";

    

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="auditLogService">The audit log service.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="cacheService">The cache service.</param>
    public UserService(
        UserManager<ApplicationUser> userManager,
        ILogger<UserService> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        IUserRepository userRepository,
        ICacheService cacheService,
        IValueListService valueListService)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _valueListService = valueListService ?? throw new ArgumentNullException(nameof(valueListService));
    }

    /// <summary>
    /// Gets the current user's unique identifier from claims.
    /// </summary>
    /// <returns>The current user's Guid.</returns>
    /// <exception cref="InvalidOperationException">Thrown if user id claim is not found.</exception>
    public Guid GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdStr = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr))
            throw new InvalidOperationException("User id claim not found in current context.");
        if (!Guid.TryParse(userIdStr, out var userId))
            throw new InvalidOperationException("User id claim is not a valid Guid.");
        return userId;
    }

    private string GetUserIp() =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

    /// <summary>
    /// Maps an ApplicationUser to a UserResponse.
    /// </summary>
    /// <param name="user">The application user.</param>
    /// <returns>The user response.</returns>
    private static UserResponse MapToUserResponse(ApplicationUser user)
    {
        return new UserResponse
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
    }
    
    /// <summary>
    /// Gets a display name for a time zone identifier.
    /// </summary>
    /// <param name="timeZoneId">The time zone identifier.</param>
    /// <returns>The display name for the time zone.</returns>
    private static string GetTimeZoneDisplayName(string timeZoneId)
    {
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return $"{timeZoneInfo.DisplayName} ({timeZoneId})";
        }
        catch
        {
            return timeZoneId;
        }
    }

    /// <inheritdoc />
    public async Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID: {UserId}", id);
        
        // Use the repository for efficient query that excludes sensitive data
        var userResponse = await _userRepository.GetUserByIdAsync(id, cancellationToken);
        if (userResponse == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return null;
        }

        return userResponse;
    }
    

    /// <inheritdoc />
    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, Guid? createdBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user with email: {Email}", request.Email);

        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Email cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new ValidationException("Username cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("Password cannot be null or empty.");

        // Check if user with email already exists
        var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingUserByEmail != null)
        {
            _logger.LogWarning("Attempt to create user with existing email: {Email}", request.Email);
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");
        }

        // Check if user with username already exists
        var existingUserByUsername = await _userManager.FindByNameAsync(request.UserName);
        if (existingUserByUsername != null)
        {
            _logger.LogWarning("Attempt to create user with existing username: {UserName}", request.UserName);
            throw new InvalidOperationException($"A user with username '{request.UserName}' already exists.");
        }

        var userId = Guid.CreateVersion7();
        var currentUserId = createdBy ?? CommonConstant.DEFAULT_SYSTEM_USER;

        // Create new user
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Location = request.Location,
            TimeZone = request.TimeZone ?? TimeZoneInfo.Local.Id,
            PhoneNumber = request.PhoneNumber,
            Profile = request.Profile,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId,
            UpdatedBy = currentUserId,
            IsActive = "Y",
            EmailConfirmed = true // Auto-confirm email for now
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user: {Errors}", errors);
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        // Add audit log
        await _auditLogService.CreateAuditLogAsync(
            objectKey: "user",
            @event: CommonConstant.AUDIT_LOG_EVENT_CREATE,
            objectItemId: user.Id.ToString(),
            data: $"User created: {user.FirstName} {user.LastName}",
            ip: GetUserIp(),
            createdBy: currentUserId,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Successfully created user with ID: {UserId}", user.Id);
        // Invalidate cached default user list
        await _cacheService.RemoveAsync(CommonConstant.CacheKeys.UserList);
        _logger.LogDebug("Invalidated cache key: {CacheKey}", CommonConstant.CacheKeys.UserList);
        return MapToUserResponse(user);
    }

    /// <inheritdoc />
    public async Task<UsersListResponse> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all users");

        // Try cache first
        var cached = await _cacheService.GetAsync<UsersListResponse>(CommonConstant.CacheKeys.UserList);
        if (cached != null)
        {
            return cached;
        }

        // Fetch all users without pagination
        var users = await _userRepository.GetAllUsersAsync(cancellationToken: cancellationToken);

        // Build related: profiles from value lists, statuses static
        var profileItems = await _valueListService.GetUserProfilesAsync(cancellationToken);
        var profiles = profileItems
            .Select(i => new ValueListItemOption { Id = i.Id, ItemName = i.ItemName, ItemKey = i.ItemKey })
            .OrderBy(p => p.ItemName)
            .ToArray();

        // Dictionaries for text enrichment
        var countriesLookup = await _valueListService.GetCountriesLookupAsync(cancellationToken);
        var profilesLookup = await _valueListService.GetUserProfilesLookupAsync(cancellationToken);

        // Enrich users with display texts
        var usersList = users as IList<UserResponse> ?? users.ToList();
        foreach (var u in usersList)
        {
            if (!string.IsNullOrEmpty(u.CountryCode) && countriesLookup.TryGetValue(u.CountryCode, out var countryItem))
            {
                u.CountryCodeText = countryItem.ItemName;
            }
            else
            {
                u.CountryCodeText = null;
            }

            if (!string.IsNullOrEmpty(u.Profile) && profilesLookup.TryGetValue(u.Profile, out var profileItem))
            {
                u.ProfileText = profileItem.ItemName;
            }
            else
            {
                u.ProfileText = null;
            }
        }

        var statuses = _valueListService.GetStatuses().ToArray();

        var response = new UsersListResponse
        {
            Data = usersList,
            Related = new UsersListRelated
            {
                Profiles = profiles,
                Statuses = statuses
            }
        };

        // Cache the complete list
        await _cacheService.SetAsync(CommonConstant.CacheKeys.UserList, response);

        return response;
    }
}
