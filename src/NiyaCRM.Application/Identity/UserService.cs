using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.Identity.DTOs;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using NiyaCRM.Core.Cache;

namespace NiyaCRM.Application.Identity;

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
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheService _cacheService;
    
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
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="cacheService">The cache service.</param>
    public UserService(
        UserManager<ApplicationUser> userManager,
        ILogger<UserService> logger,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        IUserRepository userRepository,
        IMemoryCache memoryCache,
        ICacheService cacheService)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
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
            TimeZone = user.TimeZone,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive == "Y",
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy
        };
    }
    
    /// <summary>
    /// Converts a UserResponse to a UserResponseWithDisplay.
    /// </summary>
    /// <param name="user">The user response.</param>
    /// <returns>The user response with display values.</returns>
    private async Task<UserResponseWithDisplay> ConvertToUserResponseWithDisplay(UserResponse user, CancellationToken cancellationToken = default)
    {
        return new UserResponseWithDisplay
        {
            Id = new ValueDisplayPair<Guid> { Value = user.Id, DisplayValue = user.Id.ToString() },
            Email = new ValueDisplayPair<string> { Value = user.Email, DisplayValue = user.Email },
            UserName = new ValueDisplayPair<string> { Value = user.UserName, DisplayValue = user.UserName },
            FirstName = new ValueDisplayPair<string> { Value = user.FirstName, DisplayValue = user.FirstName ?? string.Empty },
            LastName = new ValueDisplayPair<string> { Value = user.LastName, DisplayValue = user.LastName ?? string.Empty },
            TimeZone = new ValueDisplayPair<string> { Value = user.TimeZone, DisplayValue = GetTimeZoneDisplayName(user.TimeZone) },
            PhoneNumber = new ValueDisplayPair<string> { Value = user.PhoneNumber, DisplayValue = user.PhoneNumber ?? string.Empty },
            IsActive = new ValueDisplayPair<bool> { Value = user.IsActive, DisplayValue = user.IsActive ? "Active" : "Inactive" },
            CreatedAt = new ValueDisplayPair<DateTime> { Value = user.CreatedAt, DisplayValue = user.CreatedAt.ToString("g", CultureInfo.InvariantCulture) },
            UpdatedAt = new ValueDisplayPair<DateTime> { Value = user.UpdatedAt, DisplayValue = user.UpdatedAt.ToString("g", CultureInfo.InvariantCulture) },
            CreatedBy = new ValueDisplayPair<Guid> { Value = user.CreatedBy, DisplayValue = await GetUserFullNameFromCacheAsync(user.CreatedBy, cancellationToken) },
            UpdatedBy = new ValueDisplayPair<Guid> { Value = user.UpdatedBy, DisplayValue = await GetUserFullNameFromCacheAsync(user.UpdatedBy, cancellationToken) }
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
        var currentUserId = createdBy ?? CommonConstant.DEFAULT_TECHNICAL_USER;

        // Create new user
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TimeZone = request.TimeZone ?? TimeZoneInfo.Local.Id,
            PhoneNumber = request.PhoneNumber,
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
        return MapToUserResponse(user);
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
    public async Task<UserResponseWithDisplay?> GetUserByIdWithDisplayAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID with display values: {UserId}", id);
        
        var userResponse = await GetUserByIdAsync(id, cancellationToken);
        if (userResponse == null)
        {
            return null;
        }
        
        // Transform the response to include display values
        return await ConvertToUserResponseWithDisplay(userResponse, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        if (pageNumber < CommonConstant.PAGE_NUMBER_DEFAULT)
            throw new ArgumentException($"Page number must be greater than {CommonConstant.PAGE_NUMBER_DEFAULT}.", nameof(pageNumber));
        
        if (pageSize < CommonConstant.PAGE_SIZE_MIN || pageSize > CommonConstant.PAGE_SIZE_MAX)
            throw new ArgumentException($"Page size must be between {CommonConstant.PAGE_SIZE_MIN} and {CommonConstant.PAGE_SIZE_MAX}.", nameof(pageSize));

        _logger.LogDebug("Getting all users - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        
        // Use the repository for efficient paginated query that excludes sensitive data
        return await _userRepository.GetAllUsersAsync(pageNumber, pageSize, cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<UserResponseWithDisplay>> GetAllUsersWithDisplayAsync(int pageNumber = CommonConstant.PAGE_NUMBER_DEFAULT, int pageSize = CommonConstant.PAGE_SIZE_DEFAULT, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all users with display values - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        // Get users from repository
        var users = await _userRepository.GetAllUsersAsync(pageNumber, pageSize, cancellationToken);
        
        // Transform each user to include display values
        var tasks = users.Select(user => ConvertToUserResponseWithDisplay(user, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
    
    /// <inheritdoc />
    public async Task CacheAllUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Caching all users individually");
            
        // Get all users from repository without pagination
        var users = await _userRepository.GetAllUsersWithoutPaginationAsync(cancellationToken);
        
        // First, cache all raw user data
        foreach (var user in users)
        {            
            // Cache each user individually with key "user_{guid}"
            string cacheKey = $"{USER_CACHE_KEY_PREFIX}{user.Id}";
            await _cacheService.SetAsync(cacheKey, user);
            
            _logger.LogDebug("User cached with key: {CacheKey}", cacheKey);
        }
        
        _logger.LogDebug("All users cached individually. Count: {Count}", users.Count().ToString());
    }
    
    /// <inheritdoc />
    public async Task<UserResponseWithDisplay?> GetUserFromCacheAsync(Guid id, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"{USER_CACHE_KEY_PREFIX}{id}";
        _logger.LogDebug("Getting user from cache with key: {CacheKey}", cacheKey);

        int attempt = 1;
        
        // Get raw user data from cache
        var cachedUser = await _cacheService.GetAsync<UserResponse>(cacheKey);
        
        while (cachedUser == null && attempt <= 3)
        {
            _logger.LogDebug("User not found in cache with key: {CacheKey}", cacheKey);
            await CacheAllUsersAsync(cancellationToken);
            cachedUser = await _cacheService.GetAsync<UserResponse>(cacheKey);
            attempt++;
        }
        
        if (cachedUser == null)
        {
            _logger.LogDebug("User not found in cache with key: {CacheKey}", cacheKey);
            return null;
        }
        
        _logger.LogDebug("User retrieved from cache with key: {CacheKey}", cacheKey);
        
        // Convert raw user data to display format without async operations
        var userWithDisplay = new UserResponseWithDisplay
        {
            Id = new ValueDisplayPair<Guid> { Value = cachedUser.Id, DisplayValue = cachedUser.Id.ToString() },
            UserName = new ValueDisplayPair<string> { Value = cachedUser.UserName, DisplayValue = cachedUser.UserName ?? string.Empty },
            Email = new ValueDisplayPair<string> { Value = cachedUser.Email, DisplayValue = cachedUser.Email ?? string.Empty },
            FirstName = new ValueDisplayPair<string> { Value = cachedUser.FirstName, DisplayValue = cachedUser.FirstName ?? string.Empty },
            LastName = new ValueDisplayPair<string> { Value = cachedUser.LastName, DisplayValue = cachedUser.LastName ?? string.Empty },
            TimeZone = new ValueDisplayPair<string> { Value = cachedUser.TimeZone, DisplayValue = GetTimeZoneDisplayName(cachedUser.TimeZone) },
            PhoneNumber = new ValueDisplayPair<string> { Value = cachedUser.PhoneNumber, DisplayValue = cachedUser.PhoneNumber ?? string.Empty },
            IsActive = new ValueDisplayPair<bool> { Value = cachedUser.IsActive, DisplayValue = cachedUser.IsActive ? "Active" : "Inactive" },
            CreatedAt = new ValueDisplayPair<DateTime> { Value = cachedUser.CreatedAt, DisplayValue = cachedUser.CreatedAt.ToString("g", CultureInfo.InvariantCulture) },
            UpdatedAt = new ValueDisplayPair<DateTime> { Value = cachedUser.UpdatedAt, DisplayValue = cachedUser.UpdatedAt.ToString("g", CultureInfo.InvariantCulture) },
            CreatedBy = new ValueDisplayPair<Guid> { Value = cachedUser.CreatedBy, DisplayValue = cachedUser.CreatedBy.ToString() },
            UpdatedBy = new ValueDisplayPair<Guid> { Value = cachedUser.UpdatedBy, DisplayValue = cachedUser.UpdatedBy.ToString() }
        };
        
        return userWithDisplay;
    }
    
    /// <inheritdoc />
    public async Task<string> GetUserFullNameFromCacheAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user full name from cache for user ID: {UserId}", id);
        
        // Get raw user data directly from cache
        string cacheKey = $"{USER_CACHE_KEY_PREFIX}{id}";
        var user = await _cacheService.GetAsync<UserResponse>(cacheKey);
        
        if (user == null)
        {
            _logger.LogDebug("User not found in cache for ID: {UserId}", id);
            
            // Try to cache all users if not found
            await CacheAllUsersAsync(cancellationToken);
            user = await _cacheService.GetAsync<UserResponse>(cacheKey);
            
            if (user == null)
            {
                return string.Empty;
            }
        }
        
        // Get first name and last name values directly from the user response
        string? firstName = user.FirstName;
        string? lastName = user.LastName;
        
        // Concatenate first name and last name with a space in between
        string fullName = string.Join(" ", new[] { firstName, lastName }.Where(n => !string.IsNullOrEmpty(n)));
        
        _logger.LogDebug("Retrieved full name '{FullName}' for user ID: {UserId}", fullName, id);
        
        return !string.IsNullOrWhiteSpace(fullName) ? fullName : string.Empty;
    }
}
