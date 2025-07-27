using Microsoft.Extensions.Logging;
using NiyaCRM.Core;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.Onboarding;
using NiyaCRM.Core.Onboarding.DTOs;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Application.Tenants;
using Microsoft.AspNetCore.Identity;
using NiyaCRM.Core.Identity;
using System.Text.Json;

namespace NiyaCRM.Application.Onboarding;

/// <summary>
/// Service implementation for application onboarding and installation operations.
/// </summary>
public class OnboardingService : IOnboardingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantService _tenantService;
    private readonly ILogger<OnboardingService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnboardingService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public OnboardingService(
        IUnitOfWork unitOfWork,
        ITenantService tenantService,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<OnboardingService> logger)
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

            var userId = Guid.NewGuid();

            // 1. Create the initial admin user
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = installationDto.AdminEmail,
                Email = installationDto.AdminEmail,
                FirstName = installationDto.AdminFirstName,
                LastName = installationDto.AdminLastName,
                TimeZone = installationDto.TimeZone,
                IsActive = "Y",
                CreatedBy = userId, 
                UpdatedBy = userId
            };

            var createResult = await _userManager.CreateAsync(user, installationDto.AdminPassword);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create initial admin user: {Errors}", JsonSerializer.Serialize(createResult.Errors));
                throw new InvalidOperationException("Failed to create initial admin user");
            }

            // Assign the Admin role if it exists
            if (await _roleManager.RoleExistsAsync("Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            _logger.LogInformation("Created initial admin user with email: {Email}", installationDto.AdminEmail);

            // 2. Create the tenant using TenantService to ensure proper audit logging
            var tenant = await _tenantService.CreateTenantAsync(
                name: installationDto.TenantName,
                host: installationDto.Host,
                email: installationDto.AdminEmail,
                userId: user.Id,
                timeZone: installationDto.TimeZone,
                createdBy: user.Id,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Created initial tenant with ID: {TenantId}", tenant.Id);

            

            // Store the installation completion flag
            // For now we can just store it as a tenant property or other mechanism
            // This can be replaced with a proper setting mechanism later

            // Commit the transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Application installation completed successfully");
            return tenant;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during application installation: {Message}", ex.Message);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
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
