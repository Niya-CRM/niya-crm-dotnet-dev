using Microsoft.Extensions.Logging;
using NiyaCRM.Core;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.ApplicationSetup;
using NiyaCRM.Core.ApplicationSetup.DTOs;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Application.Tenants;
using Microsoft.AspNetCore.Identity;
using NiyaCRM.Core.Identity;
using System.Text.Json;

namespace NiyaCRM.Application.ApplicationSetup;

/// <summary>
/// Service implementation for application setup and installation operations.
/// </summary>
public class ApplicationSetupService : IApplicationSetupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantService _tenantService;
    private readonly ILogger<ApplicationSetupService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationSetupService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="roleManager">The role manager.</param>
    /// <param name="logger">The logger.</param>
    public ApplicationSetupService(
        IUnitOfWork unitOfWork,
        ITenantService tenantService,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<ApplicationSetupService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    /// <inheritdoc/>
    public async Task<Tenant> InstallApplicationAsync(AppInstallationDto installationDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(installationDto);

        _logger.LogInformation("Starting application installation process for tenant: {TenantName}", installationDto.TenantName);

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

            // Create the technical user (inactive)
            var technicalUser = await CreateTechnicalUserAsync();
            
            // Create the initial admin user
            await CreateInitialAdminUserAsync(installationDto, technicalUser.Id);

            // Create the tenant
            var tenant = await CreateInitialTenantAsync(installationDto, technicalUser.Id, cancellationToken);

            // Commit the transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Application installation completed successfully");
            return tenant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application installation");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc/>
    private async Task CreateInitialAdminUserAsync(AppInstallationDto installationDto, Guid technicalUserId)
    {
        var userId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = installationDto.AdminEmail,
            Email = installationDto.AdminEmail,
            FirstName = installationDto.FirstName,
            LastName = installationDto.LastName,
            TimeZone = installationDto.TimeZone,
            IsActive = "Y",
            CreatedBy = technicalUserId,
            UpdatedBy = technicalUserId
        };

        var createResult = await _userManager.CreateAsync(user, installationDto.Password);
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

        _logger.LogInformation("Created initial admin user with email: {Email}", installationDto.AdminEmail);
    }
    
    /// <inheritdoc/>
    private async Task<ApplicationUser> CreateTechnicalUserAsync()
    {
        var techUserId = Guid.NewGuid();
        
        var technicalUser = new ApplicationUser
        {
            Id = techUserId,
            UserName = "NiyaCRM@system.local",
            Email = "NiyaCRM@system.local",
            FirstName = "Technical",
            LastName = "Interface",
            TimeZone = "UTC",
            IsActive = "N", // Inactive user
            CreatedBy = techUserId,
            UpdatedBy = techUserId
        };

        // Use a dummy password for the technical user
        var dummyPassword = Guid.NewGuid().ToString() + "!Aa1";
        var createResult = await _userManager.CreateAsync(technicalUser, dummyPassword);
        
        if (!createResult.Succeeded)
        {
            _logger.LogError("Failed to create technical user: {Errors}", JsonSerializer.Serialize(createResult.Errors));
            throw new InvalidOperationException("Failed to create technical user");
        }

        _logger.LogInformation("Created technical user with ID: {UserId}", technicalUser.Id);
        return technicalUser;
    }

    /// <inheritdoc/>
    private async Task<Tenant> CreateInitialTenantAsync(AppInstallationDto installationDto, Guid technicalUserId, CancellationToken cancellationToken = default)
    {
        var createTenantRequest = new NiyaCRM.Core.Tenants.DTOs.CreateTenantRequest
        {
            Name = installationDto.TenantName,
            Host = installationDto.Host,
            Email = installationDto.AdminEmail,
            UserId = technicalUserId,
            TimeZone = installationDto.TimeZone
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
