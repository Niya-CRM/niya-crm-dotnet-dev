using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NiyaCRM.Core.AppInstallation;
using NiyaCRM.Core.AppInstallation.AppInitialisation;
using NiyaCRM.Core.Identity;
using NiyaCRM.Core.DynamicObjects;
using NiyaCRM.Infrastructure.Data;
using NiyaCRM.Core.Common;

namespace NiyaCRM.AppInstallation.Services
{
    public class AppInitialisationService : IAppInitialisationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<NiyaCRM.Core.Identity.ApplicationRole> _roleManager;
        private readonly NiyaCRM.Core.Identity.IPermissionRepository _permissionRepository;
        private readonly UserManager<NiyaCRM.Core.Identity.ApplicationUser> _userManager;
        private readonly ILogger<AppInitialisationService> _logger;
        
        // Static dictionary of steps
        // Key: (pipeline, version, order), Value: (step name, step action)
        private List<(string Pipeline, string Version, int Order, string Step, Func<Task> Action)> Steps =>
        [
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 1, "Seed Default Roles", async () => {
                await SeedDefaultRolesAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 2, "Seed Default Permissions", async () => {
                await SeedDefaultPermissionsAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 3, "Create Technical User", async () => {
                await CreateTechnicalUserAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 4, "Assign Permissions To Roles", async () => {
                await AssignPermissionsToRolesAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 5, "Define Dynamic Objects", async () => {
                await DefineDynamicObjectsAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 6, "Initialize Countries", async () => {
                await InitializeCountriesAsync();
            }),
        ];

        public AppInitialisationService(ApplicationDbContext dbContext, 
            RoleManager<NiyaCRM.Core.Identity.ApplicationRole> roleManager, 
            NiyaCRM.Core.Identity.IPermissionRepository permissionRepository,
            UserManager<NiyaCRM.Core.Identity.ApplicationUser> userManager,
            ILogger<AppInitialisationService> logger)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _permissionRepository = permissionRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task InitialiseAppAsync(CancellationToken cancellationToken = default)
        {
            // Check if any status records exist
            if (!await _dbContext.AppInstallationStatus.AnyAsync(cancellationToken))
            {
                // Seed steps
                foreach (var step in Steps)
                {
                    _dbContext.AppInstallationStatus.Add(new AppInstallationStatus
                    {
                        Pipeline = step.Pipeline,
                        Version = step.Version,
                        Order = step.Order,
                        Step = step.Step,
                        Completed = "N"
                    });
                }
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // Run each step that is not completed
            var statuses = await _dbContext.AppInstallationStatus.Where(s => s.Completed != "Y").ToListAsync(cancellationToken);
            foreach (var status in statuses)
            {
                var step = Steps.FirstOrDefault(x => x.Pipeline == status.Pipeline && x.Version == status.Version && x.Order == status.Order && x.Step == status.Step);
                if (step.Action != null)
                {
                    await step.Action();
                    status.Completed = "Y";
                }
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedDefaultRolesAsync()
        {
            var defaultUser = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER;
            var utcNow = DateTime.UtcNow;
            
            foreach (var role in NiyaCRM.Core.Common.CommonConstant.RoleNames.All)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var newRole = new NiyaCRM.Core.Identity.ApplicationRole(role)
                    {
                        CreatedBy = defaultUser,
                        UpdatedBy = defaultUser,
                        CreatedAt = utcNow,
                        UpdatedAt = utcNow
                    };
                    
                    await _roleManager.CreateAsync(newRole);
                }
            }
        }

        private async Task SeedDefaultPermissionsAsync()
        {
            var defaultUser = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER;
            var utcNow = DateTime.UtcNow;
            
            foreach (var permissionName in NiyaCRM.Core.Common.CommonConstant.PermissionNames.All)
            {
                // Check if permission already exists by normalized name (uppercase)
                var normalizedName = permissionName.ToUpperInvariant();
                var existingPermission = await _permissionRepository.GetByNameAsync(normalizedName);
                
                if (existingPermission == null)
                {
                    // Create new permission
                    var permission = new NiyaCRM.Core.Identity.Permission
                    {
                        Id = Guid.CreateVersion7(),
                        Name = permissionName,
                        NormalizedName = normalizedName,
                        CreatedBy = defaultUser,
                        CreatedAt = utcNow,
                        UpdatedBy = defaultUser,
                        UpdatedAt = utcNow
                    };
                    
                    await _permissionRepository.AddAsync(permission);
                }
            }
        }

        private async Task CreateTechnicalUserAsync()
        {
            // Check if technical user already exists
            var technicalUserEmail = "NiyaCRM@system.local";
            var existingUser = await _userManager.FindByEmailAsync(technicalUserEmail);
            
            if (existingUser == null)
            {
                // Create the technical user
                var technicalUser = new NiyaCRM.Core.Identity.ApplicationUser
                {
                    Id = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER,
                    UserName = technicalUserEmail,
                    Email = technicalUserEmail,
                    FirstName = "Technical",
                    LastName = "Interface",
                    CountryCode = "IN",
                    TimeZone = "UTC",
                    IsActive = "N", // Inactive user
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER
                };
                
                // Generate a strong random password
                var password = Guid.CreateVersion7().ToString() + "!Aa1";
                
                // Create the user
                var result = await _userManager.CreateAsync(technicalUser, password);
                
                if (result.Succeeded)
                {
                    // Instead of using UserManager.AddToRoleAsync, we'll create the user role directly
                    // to set the audit fields
                    var defaultUser = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER;
                    var utcNow = DateTime.UtcNow;
                    
                    // Get the role ID
                    var role = await _roleManager.FindByNameAsync(NiyaCRM.Core.Common.CommonConstant.RoleNames.Technical);
                    if (role == null)
                    {
                        _logger.LogCritical("Could not find Technical role when assigning to technical user");
                        return;
                    }
                    
                    // Create a new ApplicationUserRole with audit fields
                    var userRole = new NiyaCRM.Core.Identity.ApplicationUserRole
                    {
                        UserId = technicalUser.Id,
                        RoleId = role.Id,
                        CreatedBy = defaultUser,
                        CreatedAt = utcNow,
                        UpdatedBy = defaultUser,
                        UpdatedAt = utcNow
                    };
                    
                    try
                    {
                        // Add the user role directly to the database
                        _dbContext.UserRoles.Add(userRole);
                        await _dbContext.SaveChangesAsync();
                        
                        _logger.LogInformation("Added technical user to Technical role with audit fields");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "Failed to add technical user to Technical role");
                    }
                }
                else
                {
                    // Log the error
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogCritical("Failed to create technical user: {Errors}", errors);
                }
            }
        }
        
        private async Task DefineDynamicObjectsAsync()
        {
            _logger.LogInformation("Starting to define dynamic objects");
            
            // Define the dynamic objects with their properties
            var dynamicObjects = new List<NiyaCRM.Core.DynamicObjects.DynamicObject>
            {
                // User dynamic object
                new(
                    Guid.CreateVersion7(),
                    NiyaCRM.Core.DynamicObjects.DynamicObjectConstants.DynamicObjectNames.User,
                    "User",
                    "Users",
                    "user",
                    "System user entity for authentication and authorization",
                    NiyaCRM.Core.DynamicObjects.DynamicObjectConstants.ObjectTypes.Dedicated,
                    NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER
                ),
                
                // Account dynamic object
                new(
                    Guid.CreateVersion7(),
                    NiyaCRM.Core.DynamicObjects.DynamicObjectConstants.DynamicObjectNames.Account,
                    "Account",
                    "Accounts",
                    "account",
                    "Business or organization account",
                    NiyaCRM.Core.DynamicObjects.DynamicObjectConstants.ObjectTypes.Dedicated,
                    NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER
                ),
                
                // Contact dynamic object
                new(
                    Guid.CreateVersion7(),
                    NiyaCRM.Core.DynamicObjects.DynamicObjectConstants.DynamicObjectNames.Contact,
                    "Contact",
                    "Contacts",
                    "contact",
                    "Individual contact associated with accounts",
                    NiyaCRM.Core.DynamicObjects.DynamicObjectConstants.ObjectTypes.Dedicated,
                    NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER
                ),
                
                // Ticket dynamic object
                new(
                    Guid.CreateVersion7(),
                    NiyaCRM.Core.DynamicObjects.DynamicObjectConstants.DynamicObjectNames.Ticket,
                    "Ticket",
                    "Tickets",
                    "ticket",
                    "Support or service ticket",
                    NiyaCRM.Core.DynamicObjects.DynamicObjectConstants.ObjectTypes.Dedicated,
                    NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER
                )
            };
            
            try
            {
                // Check if each dynamic object already exists
                foreach (var dynamicObject in dynamicObjects)
                {
                    var existingObject = await _dbContext.DynamicObjects
                        .FirstOrDefaultAsync(d => d.ObjectName == dynamicObject.ObjectName);
                    
                    if (existingObject == null)
                    {
                        // Add the dynamic object if it doesn't exist
                        _dbContext.DynamicObjects.Add(dynamicObject);
                        _logger.LogInformation("Added dynamic object: {ObjectName}", dynamicObject.ObjectName);
                    }
                    else
                    {
                        _logger.LogInformation("Dynamic object already exists: {ObjectName}", dynamicObject.ObjectName);
                    }
                }
                
                // Save changes to the database
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully defined dynamic objects");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,"Failed to define dynamic objects");
            }
        }
        
        private async Task InitializeCountriesAsync()
        {
            _logger.LogInformation("Starting to initialize countries from JSON file");
            
            try
            {
                // Read countries from JSON file
                string jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "countries.json");
                
                // If the file is not found in the output directory, try to find it in the project directory
                if (!File.Exists(jsonFilePath))
                {
                    string projectDir = AppDomain.CurrentDomain.BaseDirectory;
                    // Navigate up to find the project directory
                    var parentDir = Directory.GetParent(projectDir);
                    while (parentDir != null && !Directory.Exists(Path.Combine(projectDir, "Data")))
                    {
                        projectDir = parentDir.FullName;
                        parentDir = Directory.GetParent(projectDir);
                    }
                    
                    jsonFilePath = Path.Combine(projectDir, "Data", "countries.json");
                }
                
                _logger.LogInformation("Reading countries from: {FilePath}", jsonFilePath);
                
                if (!File.Exists(jsonFilePath))
                {
                    _logger.LogCritical("Countries JSON file not found at: {FilePath}", jsonFilePath);
                    return;
                }
                
                string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                
                // Deserialize JSON to country objects
                var jsonCountries = System.Text.Json.JsonSerializer.Deserialize<List<CountryJsonModel>>(jsonContent);
                
                if (jsonCountries == null || jsonCountries.Count == 0)
                {
                    _logger.LogError("No countries found in JSON file or deserialization failed");
                    return;
                }
                
                _logger.LogInformation("Found {Count} countries in JSON file", jsonCountries.Count);
                
                // Convert JSON model to Country entities
                var countries = jsonCountries.Select(c => new NiyaCRM.Core.Referentials.Country(
                    c.Country_Name?.Trim() ?? string.Empty,
                    c.Country_Code?.Trim() ?? string.Empty,
                    c.Country_Code_Alpha_3?.Trim() ?? string.Empty,
                    c.Active?.Trim() ?? "Y"
                )).ToList();
                
                // Check if countries already exist
                var existingCountryCodes = await _dbContext.Set<NiyaCRM.Core.Referentials.Country>()
                    .Select(c => c.CountryCode)
                    .ToListAsync();
                
                // Filter out countries that already exist
                var newCountries = countries.Where(c => !existingCountryCodes.Contains(c.CountryCode)).ToList();
                
                if (newCountries.Count > 0)
                {
                    // Add new countries
                    await _dbContext.Set<NiyaCRM.Core.Referentials.Country>().AddRangeAsync(newCountries);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Added {Count} new countries", newCountries.Count);
                }
                else
                {
                    _logger.LogInformation("No new countries to add");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,"Failed to initialize countries");
            }
        }
        
        // Model class for deserializing the JSON country data
        private sealed class CountryJsonModel
        {
            public string? Country_Name { get; init; }
            public string? Country_Code { get; init; }
            public string? Country_Code_Alpha_3 { get; init; }
            public string? Active { get; init; }
        }
        
        private async Task AssignPermissionsToRolesAsync()
        {
            // Define role-based permissions
            var rolePermissions = new Dictionary<string, string[]>
            {
                // Technical role has all permissions
                [NiyaCRM.Core.Common.CommonConstant.RoleNames.Technical] = NiyaCRM.Core.Common.CommonConstant.PermissionNames.All,
                
                // Administrator has all permissions
                [NiyaCRM.Core.Common.CommonConstant.RoleNames.Administrator] = NiyaCRM.Core.Common.CommonConstant.PermissionNames.All,
                
                // Power User has most permissions except system setup
                [NiyaCRM.Core.Common.CommonConstant.RoleNames.PowerUser] = 
                [
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.UserRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.UserWrite,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TicketRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TicketWrite,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.ContactRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.ContactWrite,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.AccountRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.AccountWrite,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TemplateRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TemplateWrite
                ],
                
                // Support Agent has ticket and contact permissions
                [NiyaCRM.Core.Common.CommonConstant.RoleNames.SupportAgent] = 
                [
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TicketRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TicketWrite,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.ContactRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.ContactWrite,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.AccountRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.AccountWrite,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TemplateRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TemplateWrite
                ],
                
                // Light Agent has read-only permissions
                [NiyaCRM.Core.Common.CommonConstant.RoleNames.LightAgent] = 
                [
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TicketRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.ContactRead,
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.AccountRead
                ],
                
                // External User has minimal permissions
                [NiyaCRM.Core.Common.CommonConstant.RoleNames.ExternalUser] = 
                [
                    NiyaCRM.Core.Common.CommonConstant.PermissionNames.TicketRead
                ]
            };
            
            // Get all permissions from the repository after ensuring they're seeded
            var allPermissions = await _permissionRepository.GetAllAsync();
            var permissionDict = allPermissions.ToDictionary(p => p.Name, p => p);
            
            // Log the permissions found for debugging
            _logger.LogInformation("Found {Count} permissions in database", permissionDict.Count);
            foreach (var perm in permissionDict.Keys)
            {
                _logger.LogDebug("Permission in database: {Permission}", perm);
            }
            
            // For each role, assign the appropriate permissions as claims
            foreach (var rolePair in rolePermissions)
            {
                var roleName = rolePair.Key;
                var permissions = rolePair.Value;
                
                // Get the role
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning("Role {RoleName} not found when assigning permissions", roleName);
                    continue;
                }
                
                // Get existing claims for the role
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                
                // Add each permission as a claim if it doesn't already exist
                foreach (var permissionName in permissions)
                {
                    // Check if the permission exists in our database
                    if (!permissionDict.TryGetValue(permissionName, out var permission))
                    {
                        _logger.LogWarning("Permission {PermissionName} not found in dictionary when assigning to role {RoleName}, creating it now", permissionName, roleName);
                        
                        // Create the permission if it doesn't exist
                        var defaultUser = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER;
                        var utcNow = DateTime.UtcNow;
                        var normalizedName = permissionName.ToUpperInvariant();
                        
                        permission = new NiyaCRM.Core.Identity.Permission
                        {
                            Id = Guid.CreateVersion7(),
                            Name = permissionName,
                            NormalizedName = normalizedName,
                            CreatedBy = defaultUser,
                            CreatedAt = utcNow,
                            UpdatedBy = defaultUser,
                            UpdatedAt = utcNow
                        };
                        
                        await _permissionRepository.AddAsync(permission);
                        permissionDict[permissionName] = permission;
                        
                        _logger.LogInformation("Created missing permission: {Permission}", permissionName);
                    }
                    
                    // Check if the claim already exists
                    var claimType = "permission";
                    var claimValue = permissionName;
                    
                    if (!existingClaims.Any(c => c.Type == claimType && c.Value == claimValue))
                    {
                        // Create the claim directly with audit fields
                        var defaultUser = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER;
                        var utcNow = DateTime.UtcNow;
                        
                        // Create a new ApplicationRoleClaim with audit fields
                        var roleClaim = new NiyaCRM.Core.Identity.ApplicationRoleClaim
                        {
                            RoleId = role.Id,
                            ClaimType = claimType,
                            ClaimValue = claimValue,
                            CreatedBy = defaultUser,
                            CreatedAt = utcNow,
                            UpdatedBy = defaultUser,
                            UpdatedAt = utcNow
                        };
                        
                        try
                        {
                            // Add the claim directly to the database
                            _dbContext.RoleClaims.Add(roleClaim);
                            await _dbContext.SaveChangesAsync();
                            
                            _logger.LogInformation("Added permission {Permission} to role {Role} with audit fields", 
                                permissionName, roleName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical(ex,"Failed to add permission {Permission} to role {Role}", 
                                permissionName, roleName);
                        }
                    }
                }
            }
            
            _logger.LogInformation("Completed assigning permissions to roles");
        }
    }
}
