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
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using OXDesk.Core.Cache;
using OXDesk.Core.ValueLists;
using OXDesk.Core.ValueLists.DTOs;
using OXDesk.Core.Common.DTOs;
using OXDesk.Application.Common.Helpers;

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
    private const string USER_ENTITIES_CACHE_KEY = "user:entities:all";

    

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
    /// Builds a display name for the given user (FirstName LastName fallback to Email/UserName/Id).
    /// </summary>
    private static string BuildUserDisplayName(ApplicationUser user)
    {
        var first = user.FirstName?.Trim();
        var last = user.LastName?.Trim();
        var full = string.Join(" ", new[] { first, last }.Where(s => !string.IsNullOrEmpty(s)));
        if (!string.IsNullOrEmpty(full)) return full;
        return user.Email ?? user.UserName ?? user.Id.ToString();
    }

    /// <summary>
    /// Gets a user's display name by Id.
    /// </summary>
    private async Task<string?> GetUserDisplayNameAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user == null ? null : BuildUserDisplayName(user);
    }
    
    /// <summary>
    /// Returns the full list of ApplicationUser entities either from cache or, if not present,
    /// loads from the database and caches the result.
    /// </summary>
    private async Task<List<ApplicationUser>> GetUsersListCacheAsync(CancellationToken cancellationToken = default)
    {
        // Try cache for full ApplicationUser list
        var cachedUsers = await _cacheService.GetAsync<List<ApplicationUser>>(USER_ENTITIES_CACHE_KEY);
        if (cachedUsers != null)
        {
            return cachedUsers;
        }

        // Load from DB and cache
        var usersEntities = _userManager.Users
            .OrderBy(u => u.UserName)
            .ToList();
        await _cacheService.SetAsync(USER_ENTITIES_CACHE_KEY, usersEntities);
        return usersEntities;
    }
    
    

    /// <summary>
    /// Returns a dictionary of users keyed by their Ids for the provided set of userIds.
    /// This performs a single batched query and is suitable for lookups like CreatedBy/UpdatedBy.
    /// </summary>
    /// <param name="userIds">Collection of user IDs to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only dictionary mapping userId to ApplicationUser.</returns>
    public async Task<IReadOnlyDictionary<Guid, ApplicationUser>> GetUsersLookupByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        var idSet = new HashSet<Guid>(userIds.Where(id => id != Guid.Empty));
        if (idSet.Count == 0)
        {
            return new Dictionary<Guid, ApplicationUser>();
        }

        // Use the unified cache/DB loader so this works even when cache is cold
        var allUsers = await GetUsersListCacheAsync(cancellationToken);
        var dict = allUsers
            .Where(u => idSet.Contains(u.Id))
            .GroupBy(u => u.Id)
            .ToDictionary(g => g.Key, g => g.First());
        return dict;
    }

    /// <summary>
    /// Gets a single user's display name by Id with caching support.
    /// </summary>
    public async Task<string?> GetUserNameByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty) return null;

        // Try the cached/loaded list first
        var allUsers = await GetUsersListCacheAsync(cancellationToken);
        ApplicationUser? user = allUsers.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            user = await _userManager.FindByIdAsync(userId.ToString());
        }
        return user == null ? null : BuildUserDisplayName(user);
    }
    

    /// <inheritdoc />
    public async Task<UserDetailsResponse?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID: {UserId}", id);
        
        // Use the repository for efficient query that excludes sensitive data
        var userResponse = await _userRepository.GetUserByIdAsync(id, cancellationToken);
        if (userResponse == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return null;
        }

        // Enrich with display texts using cached lookups
        var countriesLookup = await _valueListService.GetCountriesLookupAsync(cancellationToken);
        var profilesLookup = await _valueListService.GetUserProfilesLookupAsync(cancellationToken);

        if (!string.IsNullOrEmpty(userResponse.CountryCode) && countriesLookup.TryGetValue(userResponse.CountryCode, out var countryItem))
        {
            userResponse.CountryCodeText = countryItem.ItemName;
        }
        else
        {
            userResponse.CountryCodeText = null;
        }

        if (!string.IsNullOrEmpty(userResponse.Profile) && profilesLookup.TryGetValue(userResponse.Profile, out var profileItem))
        {
            userResponse.ProfileText = profileItem.ItemName;
        }
        else
        {
            userResponse.ProfileText = null;
        }

        // Created/Updated by display names via centralized cache/DB helper
        userResponse.CreatedByText = await GetUserNameByIdAsync(userResponse.CreatedBy, cancellationToken);
        userResponse.UpdatedByText = await GetUserNameByIdAsync(userResponse.UpdatedBy, cancellationToken);

        // Build related lists
        var countries = (await _valueListService.GetCountriesAsync(cancellationToken))
            .Select(i => new ValueListItemOption { Id = i.Id, ItemName = i.ItemName, ItemKey = i.ItemKey })
            .OrderBy(c => c.ItemName)
            .ToArray();

        var profiles = (await _valueListService.GetUserProfilesAsync(cancellationToken))
            .Select(i => new ValueListItemOption { Id = i.Id, ItemName = i.ItemName, ItemKey = i.ItemKey })
            .OrderBy(p => p.ItemName)
            .ToArray();

        // Time zones using IANA IDs and friendly display names via Application helper
        var ianaTimeZones = TimeZoneHelper.GetAllIanaTimeZones();
        var timeZones = ianaTimeZones
            .Select(tz => new StringOption { Value = tz.Key, Name = tz.Value })
            .ToArray();

        var statuses = _valueListService.GetStatuses().ToArray();

        var response = new UserDetailsResponse
        {
            Data = userResponse,
            Related = new UserDetailsRelated
            {
                Countries = countries,
                Profiles = profiles,
                TimeZones = timeZones,
                Statuses = statuses
            }
        };

        return response;
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
            objectKey: CommonConstant.AUDIT_LOG_MODULE_USER,
            @event: CommonConstant.AUDIT_LOG_EVENT_CREATE,
            objectItemId: user.Id.ToString(),
            data: $"User created: {user.FirstName} {user.LastName}",
            ip: GetUserIp(),
            createdBy: currentUserId,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Successfully created user with ID: {UserId}", user.Id);
        // Invalidate cached user-related data
        await _cacheService.RemoveAsync(CommonConstant.CacheKeys.UserList);
        await _cacheService.RemoveAsync(USER_ENTITIES_CACHE_KEY);
        _logger.LogDebug("Invalidated cache keys: {CacheKey1}, {CacheKey2}", CommonConstant.CacheKeys.UserList, USER_ENTITIES_CACHE_KEY);
        return MapToUserResponse(user);
    }

    /// <inheritdoc />
    public async Task<UsersListResponse> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all users");

        // Load from cache or DB using the helper
        var usersEntities = await GetUsersListCacheAsync(cancellationToken);

        // Map to DTOs
        var users = usersEntities.Select(MapToUserResponse).ToList();

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

        // Batch resolve CreatedBy/UpdatedBy display names using a single lookup
        var distinctUserIds = new HashSet<Guid>(usersList.SelectMany(u => new[] { u.CreatedBy, u.UpdatedBy }));
        var userLookup = await GetUsersLookupByIdsAsync(distinctUserIds, cancellationToken);
        var nameMap = userLookup.ToDictionary(kvp => kvp.Key, kvp => BuildUserDisplayName(kvp.Value));

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

            // Created/Updated by display texts from name map
            u.CreatedByText = nameMap.TryGetValue(u.CreatedBy, out var createdByName) ? createdByName : null;
            u.UpdatedByText = nameMap.TryGetValue(u.UpdatedBy, out var updatedByName) ? updatedByName : null;
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

        return response;
    }

    /// <inheritdoc />
    public async Task<UserResponse> ChangeUserActivationStatusAsync(Guid id, string action, string reason, Guid? changedBy = null, CancellationToken cancellationToken = default)
    {
        bool isActivating = action.Equals(UserConstant.ActivationAction.Activate, StringComparison.OrdinalIgnoreCase);
        string actionVerb = isActivating ? "Activating" : "Deactivating";

        _logger.LogInformation("{ActionVerb} user: {UserId}", actionVerb, id);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found for {ActionVerb}: {UserId}", actionVerb.ToLower(), id);
            throw new InvalidOperationException($"User with ID '{id}' not found.");
        }

        // Update activation fields
        user.IsActive = isActivating ? "Y" : "N";
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = changedBy ?? CommonConstant.DEFAULT_SYSTEM_USER;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to update user activation status: {Errors}", errors);
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        // Audit log
        string actionPastTense = isActivating ? "activated" : "deactivated";
        await _auditLogService.CreateAuditLogAsync(
            objectKey: CommonConstant.AUDIT_LOG_MODULE_USER,
            @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
            objectItemId: user.Id.ToString(),
            data: $"User {actionPastTense}: {{ \"Reason\": \"{reason}\" }}",
            ip: GetUserIp(),
            createdBy: changedBy ?? CommonConstant.DEFAULT_SYSTEM_USER,
            cancellationToken: cancellationToken
        );

        // Invalidate cached user-related data
        await _cacheService.RemoveAsync(CommonConstant.CacheKeys.UserList);
        await _cacheService.RemoveAsync(USER_ENTITIES_CACHE_KEY);

        _logger.LogInformation("Successfully {ActionPastTense} user: {UserId}", actionPastTense, id);
        return MapToUserResponse(user);
    }
}
