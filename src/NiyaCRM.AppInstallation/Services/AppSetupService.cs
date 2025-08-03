using Microsoft.Extensions.Logging;
using NiyaCRM.Core;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.Tenants;
using Microsoft.AspNetCore.Identity;
using NiyaCRM.Core.Identity;
using System.Text.Json;
using NiyaCRM.Core.AppInstallation.AppSetup;
using NiyaCRM.Core.AppInstallation.AppSetup.DTOs;

namespace NiyaCRM.AppInstallation.Services;

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
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<AppSetupService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
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
            await CreateInitialAdminUserAsync(setupDto, CommonConstant.DEFAULT_TECHNICAL_USER);

            // Create the tenant
            var tenant = await CreateInitialTenantAsync(setupDto, CommonConstant.DEFAULT_TECHNICAL_USER, cancellationToken);

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
    private async Task CreateInitialAdminUserAsync(AppSetupDto setupDto, Guid technicalUserId)
    {
        var userId = Guid.CreateVersion7();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = setupDto.AdminEmail,
            Email = setupDto.AdminEmail,
            FirstName = setupDto.FirstName,
            LastName = setupDto.LastName,
            TimeZone = setupDto.TimeZone,
            IsActive = "Y",
            CreatedBy = technicalUserId,
            UpdatedBy = technicalUserId
        };

        var createResult = await _userManager.CreateAsync(user, setupDto.Password);
        if (!createResult.Succeeded)
        {
            _logger.LogError("Failed to create initial admin user: {Errors}", JsonSerializer.Serialize(createResult.Errors));
            throw new InvalidOperationException("Failed to create initial admin user");
        }

        // Assign the Admin role if it exists
        if (await _roleManager.RoleExistsAsync("Administrator"))
        {
            await _userManager.AddToRoleAsync(user, "Administrator");
        }

        _logger.LogInformation("Created initial admin user with email: {Email}", setupDto.AdminEmail);
    }

    /// <inheritdoc/>
    private async Task<Tenant> CreateInitialTenantAsync(AppSetupDto setupDto, Guid technicalUserId, CancellationToken cancellationToken = default)
    {
        var createTenantRequest = new NiyaCRM.Core.Tenants.DTOs.CreateTenantRequest
        {
            Name = setupDto.TenantName,
            Host = setupDto.Host,
            Email = setupDto.AdminEmail,
            UserId = technicalUserId,
            TimeZone = setupDto.TimeZone
        };

        var tenant = await _tenantService.CreateTenantAsync(
            request: createTenantRequest,
            createdBy: technicalUserId,
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
