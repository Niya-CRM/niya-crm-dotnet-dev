using Microsoft.Extensions.Logging;
using OXDesk.Core;
using OXDesk.Core.Common;
using OXDesk.Core.Tenants;
using Microsoft.AspNetCore.Identity;
using OXDesk.Core.Identity;
using System.Text.Json;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.AppInstallation.AppSetup;
using OXDesk.Core.AppInstallation.AppSetup.DTOs;
using OXDesk.Core.ValueLists;
using System.Linq;
using OXDesk.Infrastructure.Data;
using OXDesk.Core.Tenants.DTOs;

namespace OXDesk.AppInstallation.Services;

/// <summary>
/// Service implementation for application setup and installation operations.
/// </summary>
public class AppSetupService : IAppSetupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantService _tenantService;
    private readonly ILogger<AppSetupService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IValueListService _valueListService;
    private readonly IValueListItemService _valueListItemService;
    private readonly IChangeHistoryLogService _changeHistoryLogService;
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSetupService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="roleManager">The role manager.</param>
    /// <param name="logger">The logger.</param>
    public AppSetupService(
        IUnitOfWork unitOfWork,
        ITenantService tenantService,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IValueListService valueListService,
        IValueListItemService valueListItemService,
        IChangeHistoryLogService changeHistoryLogService,
        ILogger<AppSetupService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _valueListService = valueListService ?? throw new ArgumentNullException(nameof(valueListService));
        _valueListItemService = valueListItemService ?? throw new ArgumentNullException(nameof(valueListItemService));
        _changeHistoryLogService = changeHistoryLogService ?? throw new ArgumentNullException(nameof(changeHistoryLogService));
    }

    /// <inheritdoc/>
    public async Task<Tenant> InstallApplicationAsync(AppSetupDto setupDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(setupDto);

        _logger.LogInformation("Starting application installation process for tenant: {TenantName}", setupDto.TenantName);

        // Check if application is already installed
        if (await IsApplicationInstalledAsync(cancellationToken))
        {
            _logger.LogWarning("Application installation was attempted, but the system is already installed");
            throw new InvalidOperationException(CommonConstant.MESSAGE_CONFLICT + ": The application is already installed");
        }

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            // Create the initial admin user
            await CreateInitialAdminUserAsync(setupDto, CommonConstant.DEFAULT_SYSTEM_USER);

            // Create the tenant
            var tenant = await CreateInitialTenantAsync(setupDto, CommonConstant.DEFAULT_SYSTEM_USER, cancellationToken);

            // Commit the transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Application installation completed successfully");
            return tenant;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc/>
    private async Task CreateInitialAdminUserAsync(AppSetupDto setupDto, Guid systemUserId)
    {
        var userId = Guid.CreateVersion7();

        // Use the 'agent' profile key directly
        string? agentProfileKey = CommonConstant.UserProfiles.Agent.Key;

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = setupDto.AdminEmail,
            Email = setupDto.AdminEmail,
            FirstName = setupDto.FirstName,
            LastName = setupDto.LastName,
            TimeZone = setupDto.TimeZone,
            Profile = agentProfileKey,
            Location = setupDto.Location,
            CountryCode = setupDto.CountryCode,
            IsActive = "Y",
            CreatedBy = systemUserId,
            UpdatedBy = systemUserId
        };

        var createResult = await _userManager.CreateAsync(user, setupDto.Password);
        if (!createResult.Succeeded)
        {
            _logger.LogError("Failed to create initial admin user: {Errors}", JsonSerializer.Serialize(createResult.Errors));
            throw new InvalidOperationException("Failed to create initial admin user");
        }

        // Add change history log (first creation event)
        await _changeHistoryLogService.CreateChangeHistoryLogAsync(
            objectKey: CommonConstant.MODULE_USER,
            objectItemId: user.Id,
            fieldName: CommonConstant.ChangeHistoryFields.Created,
            oldValue: null,
            newValue: null,
            createdBy: systemUserId,
            cancellationToken: default
        );

        // Assign the Administrator role with audit fields (direct insert like in AppInitialisationService)
        if (await _roleManager.RoleExistsAsync(CommonConstant.RoleNames.Administrator))
        {
            var role = await _roleManager.FindByNameAsync(CommonConstant.RoleNames.Administrator);
            if (role != null)
            {
                var utcNow = DateTime.UtcNow;

                var userRole = new ApplicationUserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    CreatedBy = systemUserId,
                    CreatedAt = utcNow,
                    UpdatedBy = systemUserId,
                    UpdatedAt = utcNow
                };

                try
                {
                    _dbContext.UserRoles.Add(userRole);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Assigned Administrator role to initial admin user with audit fields");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Failed to assign Administrator role to initial admin user");
                }
            }
        }

        _logger.LogInformation("Created initial admin user with email: {Email}", setupDto.AdminEmail);
    }

    /// <inheritdoc/>
    private async Task<Tenant> CreateInitialTenantAsync(AppSetupDto setupDto, Guid systemUserId, CancellationToken cancellationToken = default)
    {
        var createTenantRequest = new CreateTenantRequest
        {
            Name = setupDto.TenantName,
            Host = setupDto.Host,
            Email = setupDto.AdminEmail,
            UserId = systemUserId,
            TimeZone = setupDto.TimeZone
        };

        var tenant = await _tenantService.CreateTenantAsync(
            request: createTenantRequest,
            createdBy: systemUserId,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Created initial tenant with ID: {TenantId}", tenant.Id);
        return tenant;
    }

    /// <inheritdoc/>
    public async Task<bool> IsApplicationInstalledAsync(CancellationToken cancellationToken = default)
    {
        // Check if any tenants exist
        // This is a simple way to determine if the application is installed
        // In a more sophisticated implementation, we might check for a specific flag or setting
        return await _tenantService.AnyTenantsExistAsync(cancellationToken);
    }
}
