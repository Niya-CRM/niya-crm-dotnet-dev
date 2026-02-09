using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using OXDesk.Core.Cache;
using OXDesk.Core.ValueLists;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.Tenants;

namespace OXDesk.Identity.Services;

/// <summary>
/// Service implementation for user management operations.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<UserService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;
    private readonly IChangeHistoryLogService _changeHistoryLogService;
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IValueListService _valueListService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IDynamicObjectService _dynamicObjectService;
    
    // Cache key prefix for users
    private const string USER_CACHE_KEY_PREFIX = "user_";
    private const string USER_ENTITIES_CACHE_KEY = "user:entities:all";

    

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="roleManager">The role manager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="auditLogService">The audit log service.</param>
    /// <param name="changeHistoryLogService">The change history log service.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="valueListService">The value list service.</param>
    /// <param name="currentTenant">The current tenant.</param>
    /// <param name="currentUser">The current user.</param>
    /// <param name="dynamicObjectService">The dynamic object service.</param>
    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<UserService> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        IChangeHistoryLogService changeHistoryLogService,
        IUserRepository userRepository,
        ICacheService cacheService,
        IValueListService valueListService,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IDynamicObjectService dynamicObjectService)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _changeHistoryLogService = changeHistoryLogService ?? throw new ArgumentNullException(nameof(changeHistoryLogService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _valueListService = valueListService ?? throw new ArgumentNullException(nameof(valueListService));
        _currentTenant = currentTenant ?? throw new ArgumentNullException(nameof(currentTenant));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _dynamicObjectService = dynamicObjectService ?? throw new ArgumentNullException(nameof(dynamicObjectService));
    }

    /// <inheritdoc />
    public int GetCurrentUserId()
    {
        var userId = _currentUser.Id;
        if (!userId.HasValue)
            throw new InvalidOperationException("User id claim not found in current context.");
        return userId.Value;
    }

    private string GetUserIp() =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

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
    private async Task<string?> GetUserDisplayNameAsync(int userId)
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
        // Use tenant-scoped cache key (tenant-implicit queries)
        var cacheKey = $"{USER_ENTITIES_CACHE_KEY}:{_currentTenant.Id?.ToString() ?? "none"}";

        // Try cache for full ApplicationUser list
        var cachedUsers = await _cacheService.GetAsync<List<ApplicationUser>>(cacheKey);
        if (cachedUsers != null)
        {
            return cachedUsers;
        }

        // Load from DB (implicitly tenant-scoped by global filters) and cache
        var usersEntities = _userManager.Users
            .OrderBy(u => u.UserName)
            .ToList();
        await _cacheService.SetAsync(cacheKey, usersEntities);
        return usersEntities;
    }
    
    

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<int, ApplicationUser>> GetUsersLookupByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        var idSet = new HashSet<int>(userIds.Where(id => id != 0));
        if (idSet.Count == 0)
        {
            return new Dictionary<int, ApplicationUser>();
        }

        // Use the unified cache/DB loader so this works even when cache is cold
        var allUsers = await GetUsersListCacheAsync(cancellationToken);
        var dict = allUsers
            .Where(u => idSet.Contains(u.Id))
            .GroupBy(u => u.Id)
            .ToDictionary(g => g.Key, g => g.First());
        return dict;
    }

    /// <inheritdoc />
    public async Task<string?> GetUserNameByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId == 0) return null;

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
    public async Task<ApplicationUser?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID: {UserId}", id);
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return null;
        }
        return user;
    }
    

    /// <inheritdoc />
    public async Task<ApplicationUser> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user with email: {Email}", request.Email);

        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Email cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new ValidationException("Username cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("Password cannot be null or empty.");

        // Check if user with email already exists in this tenant
        var existingUserByEmail = _userManager.Users
            .Where(u => u.Email == request.Email)
            .FirstOrDefault();
            
        if (existingUserByEmail != null)
        {
            _logger.LogWarning("Attempt to create user with existing email: {Email}", request.Email);
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");
        }

        // Check if user with username already exists in this tenant
        var existingUserByUsername = _userManager.Users
            .Where(u => u.UserName == request.UserName)
            .FirstOrDefault();
            
        if (existingUserByUsername != null)
        {
            _logger.LogWarning("Attempt to create user with existing username: {UserName}", request.UserName);
            throw new InvalidOperationException($"A user with username '{request.UserName}' already exists.");
        }

        // Determine actor for audit fields
        var actorId = GetCurrentUserId();

        // Create new user (tenant is implicit via global filters / DB policies)
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            MobileNumber = request.MobileNumber,
            JobTitle = request.JobTitle,
            Language = request.Language,
            Location = request.Location,
            TimeZone = request.TimeZone ?? TimeZoneInfo.Local.Id,
            PhoneNumber = request.PhoneNumber,
            Profile = request.Profile,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = actorId,
            UpdatedBy = actorId,
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
            @event: CommonConstant.AUDIT_LOG_EVENT_CREATE,
            objectKey: DynamicObjectConstants.DynamicObjectKeys.User,
            objectItemId: user.Id,
            data: $"User created: {user.FirstName} {user.LastName}",
            ip: GetUserIp(),
            createdBy: actorId,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Successfully created user with ID: {UserId}", user.Id);
        // Invalidate cached user-related data (tenant-scoped)
        var cacheKey = $"{USER_ENTITIES_CACHE_KEY}:{_currentTenant.Id?.ToString() ?? "none"}";
        await _cacheService.RemoveAsync(CommonConstant.CacheKeys.UserList);
        await _cacheService.RemoveAsync(cacheKey);
        _logger.LogDebug("Invalidated cache keys: {CacheKey1}, {CacheKey2}", CommonConstant.CacheKeys.UserList, cacheKey);
        return user;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ApplicationUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all users");

        // Load from cache or DB using the helper and return entities only
        var usersEntities = await GetUsersListCacheAsync(cancellationToken);
        return usersEntities;
    }

    /// <inheritdoc />
    public async Task<ApplicationUser> ChangeUserActivationStatusAsync(int id, string action, string reason, CancellationToken cancellationToken = default)
    {
        bool isActivating = action.Equals(UserConstant.ActivationAction.Activate, StringComparison.OrdinalIgnoreCase);
        string actionVerb = isActivating ? "Activating" : "Deactivating";

        _logger.LogInformation("{ActionVerb} user: {UserId}", actionVerb, id);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found for {ActionVerb} {UserId}", actionVerb.ToLower(), id);
            throw new InvalidOperationException($"User with ID '{id}' not found.");
        }

        if(user.Profile == CommonConstant.UserProfiles.System.Key | user.Profile == CommonConstant.UserProfiles.AIAgent.Key)
        {
            _logger.LogWarning("User {ActionVerb} is not allowed as profile is {profile} for {UserId}", actionVerb.ToLower(), user.Profile, id);
            throw new InvalidOperationException($"User {actionVerb.ToLower()} is not allowed as profile is {user.Profile} for {id}");
        }

        // Determine actor: current user from context
        var actorId = GetCurrentUserId();

        // Update activation fields
        var oldActive = user.IsActive;
        var newActive = isActivating ? "Y" : "N";
        user.IsActive = newActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = actorId;

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
            @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
            objectKey: DynamicObjectConstants.DynamicObjectKeys.User,
            objectItemId: user.Id,
            data: $"User {actionPastTense}: {{ \"Reason\": \"{reason}\" }}",
            ip: GetUserIp(),
            createdBy: actorId,
            cancellationToken: cancellationToken
        );

        // Change history log for activation status change
        await _changeHistoryLogService.CreateChangeHistoryLogAsync(
            objectKey: DynamicObjectConstants.DynamicObjectKeys.User,
            objectItemId: user.Id,
            fieldName: "isActive",
            oldValue: oldActive,
            newValue: newActive,
            createdBy: actorId,
            cancellationToken: cancellationToken
        );

        // Invalidate cached user-related data (tenant-scoped)
        var cacheKey = $"{USER_ENTITIES_CACHE_KEY}:{_currentTenant.Id?.ToString() ?? "none"}";
        await _cacheService.RemoveAsync(CommonConstant.CacheKeys.UserList);
        await _cacheService.RemoveAsync(cacheKey);

        _logger.LogInformation("Successfully {ActionPastTense} user: {UserId}", actionPastTense, id);
        return user;
    }

    public async Task<IReadOnlyList<ApplicationRole>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found when getting roles: {UserId}", userId);
            throw new InvalidOperationException($"User with ID '{userId}' not found.");
        }
        
        // Tenant scope is enforced by global query filters

        var roleNames = await _userManager.GetRolesAsync(user);
        var roles = _roleManager.Roles
            .Where(r => roleNames.Contains(r.Name ?? string.Empty))
            .OrderBy(r => r.Name)
            .ToList();
        return roles;
    }

    public async Task<IReadOnlyList<ApplicationRole>> AddRoleToUserAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found when adding role: {UserId}", userId);
            throw new InvalidOperationException($"User with ID '{userId}' not found.");
        }
        
        // Tenant scope is enforced by global query filters

        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            _logger.LogWarning("Role not found when adding to user: {RoleId}", roleId);
            throw new InvalidOperationException($"Role with ID '{roleId}' not found.");
        }

        var roleName = role.Name ?? string.Empty;
        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to assign role to user {UserId}: {Errors}", userId, errors);
                throw new InvalidOperationException($"Failed to assign role: {errors}");
            }

            var actorId = GetCurrentUserId();

            await _auditLogService.CreateAuditLogAsync(
                @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
                objectKey: DynamicObjectConstants.DynamicObjectKeys.User,
                objectItemId: user.Id,
                data: $"Role assigned to user: {role.Name}",
                ip: GetUserIp(),
                createdBy: actorId,
                cancellationToken: cancellationToken
            );

            var cacheKey = $"{USER_ENTITIES_CACHE_KEY}:{_currentTenant.Id?.ToString() ?? "none"}";
            await _cacheService.RemoveAsync(CommonConstant.CacheKeys.UserList);
            await _cacheService.RemoveAsync(cacheKey);
        }

        return await GetUserRolesAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationRole>> RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            _logger.LogWarning("User not found when removing role: {UserId}", userId);
            throw new InvalidOperationException($"User with ID '{userId}' not found.");
        }
        
        // Tenant scope is enforced by global query filters

        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            _logger.LogWarning("Role not found when removing from user: {RoleId}", roleId);
            throw new InvalidOperationException($"Role with ID '{roleId}' not found.");
        }

        var roleName = role.Name ?? string.Empty;
        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to remove role from user {UserId}: {Errors}", userId, errors);
                throw new InvalidOperationException($"Failed to remove role: {errors}");
            }

            var actorId = GetCurrentUserId();

            await _auditLogService.CreateAuditLogAsync(
                @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
                objectKey: DynamicObjectConstants.DynamicObjectKeys.User,
                objectItemId: user.Id,
                data: $"Role removed from user: {role.Name}",
                ip: GetUserIp(),
                createdBy: actorId,
                cancellationToken: cancellationToken
            );

            var cacheKey = $"{USER_ENTITIES_CACHE_KEY}:{_currentTenant.Id?.ToString() ?? "none"}";
            await _cacheService.RemoveAsync(CommonConstant.CacheKeys.UserList);
            await _cacheService.RemoveAsync(cacheKey);
        }

        return await GetUserRolesAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetUsersByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            _logger.LogWarning("Role not found when fetching users: {RoleId}", roleId);
            throw new InvalidOperationException($"Role with ID '{roleId}' not found.");
        }

        var roleName = role.Name ?? string.Empty;
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        
        // Tenant scope is enforced by global query filters
        var ordered = users.OrderBy(u => u.UserName).ToList();
        return ordered;
    }

    /// <inheritdoc />
    public async Task<int?> GetTechnicalUserIdAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting Technical User ID");
        
        var technicalUser = await _userManager.FindByNameAsync(CommonConstant.TECHNICAL_USERNAME);
        
        if (technicalUser == null)
        {
            _logger.LogWarning("Technical user not found with username: {Username}", CommonConstant.TECHNICAL_USERNAME);
            return null;
        }
        
        _logger.LogDebug("Found Technical User with ID: {UserId}", technicalUser.Id);
        return technicalUser.Id;
    }
}
