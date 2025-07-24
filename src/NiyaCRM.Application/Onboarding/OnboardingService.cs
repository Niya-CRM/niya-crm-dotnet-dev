using Microsoft.Extensions.Logging;
using NiyaCRM.Core;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.Onboarding;
using NiyaCRM.Core.Onboarding.DTOs;
using NiyaCRM.Core.Tenants;
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
    // private readonly IUserService _userService; // Will be uncommented when User service is available

    /// <summary>
    /// Initializes a new instance of the <see cref="OnboardingService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="logger">The logger.</param>
    public OnboardingService(
        IUnitOfWork unitOfWork,
        ITenantService tenantService,
        // IUserService userService, // Will be uncommented when User service is available
        ILogger<OnboardingService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        // _userService = userService ?? throw new ArgumentNullException(nameof(userService)); // Will be uncommented when User service is available
    }

    /// <inheritdoc/>
    public async Task<Tenant> InstallApplicationAsync(AppInstallationDto installationDto, CancellationToken cancellationToken = default)
    {
        if (installationDto == null)
        {
            throw new ArgumentNullException(nameof(installationDto));
        }

        _logger.LogInformation("Starting application installation process for tenant: {TenantName}", installationDto.TenantName);

        // Check if application is already installed
        if (await IsApplicationInstalledAsync(cancellationToken))
        {
            _logger.LogWarning("Application installation was attempted, but the system is already installed");
            throw new InvalidOperationException(CommonConstant.MESSAGE_CONFLICT + ": The application is already installed");
        }

        // Validate required fields
        ValidateInstallationDto(installationDto);

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // 1. Create the tenant
            var tenant = await _tenantService.CreateTenantAsync(
                name: installationDto.TenantName,
                host: installationDto.TenantHost,
                email: installationDto.TenantEmail,
                // No database name as per user's DTO modification
                createdBy: "System.Installer",
                cancellationToken: cancellationToken);

            _logger.LogInformation("Created initial tenant with ID: {TenantId}", tenant.Id);

            // 2. Create the initial admin user
            // This section is commented out as the User service is not available yet
            /*
            await _userService.CreateUserAsync(
                firstName: installationDto.AdminFirstName,
                lastName: installationDto.AdminLastName,
                email: installationDto.AdminEmail,
                password: installationDto.AdminPassword,
                tenantId: tenant.Id,
                isAdmin: true,
                timeZone: installationDto.TimeZone,
                locale: installationDto.Locale,
                createdBy: "System.Installer",
                cancellationToken: cancellationToken);

            _logger.LogInformation("Created initial admin user with email: {Email}", installationDto.AdminEmail);
            */
            // For now, log that this would happen
            _logger.LogInformation("Would create initial admin user with email: {Email} (functionality pending)", installationDto.AdminEmail);

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
            _logger.LogError(ex, "Error during application installation: {Message}", ex.Message);
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
        var tenants = await _tenantService.GetAllTenantsAsync(pageNumber: 1, pageSize: 1, cancellationToken);
        return tenants.Any();
    }

    /// <summary>
    /// Validates the installation DTO to ensure all required fields are provided.
    /// </summary>
    /// <param name="installationDto">The installation DTO to validate.</param>
    private void ValidateInstallationDto(AppInstallationDto installationDto)
    {
        var validationErrors = new List<string>();

        // Tenant validation
        if (string.IsNullOrWhiteSpace(installationDto.TenantName))
            validationErrors.Add("Tenant name is required");
        if (string.IsNullOrWhiteSpace(installationDto.TenantHost))
            validationErrors.Add("Tenant host is required");
        if (string.IsNullOrWhiteSpace(installationDto.TenantEmail))
            validationErrors.Add("Tenant email is required");

        // Admin user validation
        if (string.IsNullOrWhiteSpace(installationDto.AdminFirstName))
            validationErrors.Add("Admin first name is required");
        if (string.IsNullOrWhiteSpace(installationDto.AdminLastName))
            validationErrors.Add("Admin last name is required");
        if (string.IsNullOrWhiteSpace(installationDto.AdminEmail))
            validationErrors.Add("Admin email is required");
        if (string.IsNullOrWhiteSpace(installationDto.AdminPassword))
            validationErrors.Add("Admin password is required");

        // System preferences validation
        if (string.IsNullOrWhiteSpace(installationDto.TimeZone))
            validationErrors.Add("Time zone is required");
        if (string.IsNullOrWhiteSpace(installationDto.Locale))
            validationErrors.Add("Locale is required");

        if (validationErrors.Any())
        {
            throw new ArgumentException(
                $"{CommonConstant.MESSAGE_INVALID_REQUEST}: {string.Join("; ", validationErrors)}");
        }
    }
}
