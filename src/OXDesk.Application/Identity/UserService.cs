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
using OXDesk.Core.AuditLogs.ChangeHistory;
using System;
using OXDesk.Core.Tenants;

namespace OXDesk.Application.Identity;

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
        RoleManager<ApplicationRole> roleManager,
        ILogger<UserService> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        IChangeHistoryLogService changeHistoryLogService,
        IUserRepository userRepository,
        ICacheService cacheService,
        IValueListService valueListService,
        ICurrentTenant currentTenant)
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
    }

    /// <summary>
    /// Gets the current user's unique identifier from claims.
    /// </summary>
    /// <returns>The current user's int identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown if user id claim is not found.</exception>
    public int GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        // Prefer NameIdentifier but fallback to 'sub' commonly used by JWTs
        var userIdStr = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? user?.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdStr))
            throw new InvalidOperationException("User id claim not found in current context.");
        if (!int.TryParse(userIdStr, out var userId))
            throw new InvalidOperationException("User id claim is not a valid integer.");
        return userId;
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
    
    

    /// <summary>
    /// Returns a dictionary of users keyed by their Ids for the provided set of userIds.
    /// This performs a single batched query and is suitable for lookups like CreatedBy/UpdatedBy.
    /// </summary>
    /// <param name="userIds">Collection of user IDs to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only dictionary mapping userId to ApplicationUser.</returns>
    public async Task<IReadOnlyDictionary<int, ApplicationUser>> GetUsersLookupByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        var idSet = new HashSet<int>(userIds.Where(id => id > 0));
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

    /// <summary>
    /// Gets a single user's display name by Id with caching support.
    /// </summary>
    public async Task<string?> GetUserNameByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0) return null;

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
    public async Task<ApplicationUser> CreateUserAsync(CreateUserRequest request, int? createdBy = null, CancellationToken cancellationToken = default)
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
        var actorId = createdBy ?? CommonConstant.DEFAULT_SYSTEM_USER;

        // Create new user (tenant is implicit via global filters / DB policies)
        var user = new ApplicationUser
        {
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
            objectKey: CommonConstant.MODULE_USER,
            @event: CommonConstant.AUDIT_LOG_EVENT_CREATE,
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
    public async Task<ApplicationUser> ChangeUserActivationStatusAsync(int id, string action, string reason, int? changedBy = null, CancellationToken cancellationToken = default)
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

        // Determine actor: changedBy parameter or current user from context
        var actorId = changedBy ?? GetCurrentUserId();

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
            objectKey: CommonConstant.MODULE_USER,
            @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
            objectItemId: user.Id,
            data: $"User {actionPastTense}: {{ \"Reason\": \"{reason}\" }}",
            ip: GetUserIp(),
            createdBy: actorId,
            cancellationToken: cancellationToken
        );

        // Change history log for activation status change
        await _changeHistoryLogService.CreateChangeHistoryLogAsync(
            objectKey: CommonConstant.MODULE_USER,
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

    public async Task<IReadOnlyList<ApplicationRole>> AddRoleToUserAsync(int userId, int roleId, int? assignedBy = null, CancellationToken cancellationToken = default)
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

            var actorId = assignedBy ?? GetCurrentUserId();
            await _auditLogService.CreateAuditLogAsync(
                objectKey: CommonConstant.MODULE_USER,
                @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
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

    public async Task<IReadOnlyList<ApplicationRole>> RemoveRoleFromUserAsync(int userId, int roleId, int? removedBy = null, CancellationToken cancellationToken = default)
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

            var actorId = removedBy ?? GetCurrentUserId();
            await _auditLogService.CreateAuditLogAsync(
                objectKey: CommonConstant.MODULE_USER,
                @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
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
}

