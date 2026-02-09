using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OXDesk.Core.AppInstallation;
using OXDesk.Core.AppInstallation.AppInitialisation;
using OXDesk.Core.AppInstallation.AppSetup;
using OXDesk.Core.AppInstallation.AppSetup.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.DynamicObjects.Fields;
using OXDesk.DbContext.Data;
using OXDesk.Core.Common;
using OXDesk.Core.ValueLists;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Tenants;

namespace OXDesk.AppInstallation.Services
{
    /// <summary>
    /// Service implementation for application initialization operations.
    /// Handles seeding of default data, roles, permissions, and initial setup.
    /// </summary>
    public class AppInitialisationService : IAppInitialisationService
    {
        private readonly TenantDbContext _dbContext;
        private readonly RoleManager<OXDesk.Core.Identity.ApplicationRole> _roleManager;
        private readonly OXDesk.Core.Identity.IPermissionRepository _permissionRepository;
        private readonly UserManager<OXDesk.Core.Identity.ApplicationUser> _userManager;
        private readonly ILogger<AppInitialisationService> _logger;
        private readonly IValueListService _valueListService;
        private readonly IChangeHistoryLogService _changeHistoryLogService;
        private readonly IAppSetupService _appSetupService;
        private readonly ICurrentTenant _currentTenant;
        private readonly ICurrentUser _currentUser;
        private readonly IDynamicObjectService _dynamicObjectService;
        private readonly Guid _tenantId;
        private readonly int _technicalUserId;
        
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
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 3, "Assign Permissions To Roles", async () => {
                await AssignPermissionsToRolesAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 4, "Define Dynamic Objects", async () => {
                await DefineDynamicObjectsAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 5, "Initialize User Profiles", async () => {
                await InitializeUserProfile();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 6, "Initialize Countries", async () => {
                await InitializeCountriesAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 7, "Initialize Currencies", async () => {
                await InitializeCurrenciesAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 8, "Initialize Languages", async () => {
                await InitializeLanguagesAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 9, "Initialize Field Types", async () => {
                await InitializeFieldTypesAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 10, "Initialize Request Types", async () => {
                await InitializeRequestTypesAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 11, "Create System User", async () => {
                await CreateSystemUserAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 999, "Setup Tenant and Admin User", async () => {
                await SetupTenantAndAdminAsync();
            }),
        ];

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInitialisationService"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="roleManager">The role manager.</param>
        /// <param name="permissionRepository">The permission repository.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="valueListService">The value list service.</param>
        /// <param name="changeHistoryLogService">The change history log service.</param>
        /// <param name="appSetupService">The app setup service.</param>
        /// <param name="currentTenant">The current tenant accessor.</param>
        /// <param name="currentUser">The current user accessor.</param>
        /// <param name="logger">The logger.</param>
        public AppInitialisationService(TenantDbContext dbContext, 
            RoleManager<OXDesk.Core.Identity.ApplicationRole> roleManager, 
            OXDesk.Core.Identity.IPermissionRepository permissionRepository,
            UserManager<OXDesk.Core.Identity.ApplicationUser> userManager,
            IValueListService valueListService,
            IChangeHistoryLogService changeHistoryLogService,
            IAppSetupService appSetupService,
            ICurrentTenant currentTenant,
            ICurrentUser currentUser,
            IDynamicObjectService dynamicObjectService,
            ILogger<AppInitialisationService> logger)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _permissionRepository = permissionRepository;
            _userManager = userManager;
            _valueListService = valueListService;
            _changeHistoryLogService = changeHistoryLogService;
            _appSetupService = appSetupService;
            _currentTenant = currentTenant;
            _currentUser = currentUser;
            _logger = logger;
            _dynamicObjectService = dynamicObjectService;

            // Generate a tenant ID for initialization process
            _tenantId = Guid.CreateVersion7();
            _technicalUserId = 10001; // Reserved int ID for technical/system user
        }

        /// <inheritdoc/>
        public async Task InitialiseAppAsync(CancellationToken cancellationToken = default)
        {

            // Seed using a scoped tenant and user context
            using (_currentTenant.ChangeScoped(_tenantId))
            using (_currentUser.ChangeScoped(_technicalUserId))
            {
                _logger.LogInformation("Generated initialization tenant ID: {TenantId}", _tenantId);

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
        }

        /// <summary>
        /// Seeds default roles into the system.
        /// </summary>
        private async Task SeedDefaultRolesAsync()
        {
            var utcNow = DateTime.UtcNow;

            var roleDescriptions = new Dictionary<string, string>
            {
                { CommonConstant.RoleNames.AccountOwner, "Full access owner of the tenant account" },
                { CommonConstant.RoleNames.Administrator, "Administrative access to manage system settings and users" },
                { CommonConstant.RoleNames.PowerUser, "Advanced user with elevated permissions" },
                { CommonConstant.RoleNames.SupportAgent, "Handles customer support tickets and requests" },
                { CommonConstant.RoleNames.LightAgent, "Limited support agent with read-heavy access" },
                { CommonConstant.RoleNames.EndUser, "External customer or end-user with self-service access" },
                { CommonConstant.RoleNames.System, "Internal system role for automated operations" }
            };

            foreach (var role in OXDesk.Core.Common.CommonConstant.RoleNames.All)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    roleDescriptions.TryGetValue(role, out var description);

                    var newRole = new OXDesk.Core.Identity.ApplicationRole(role)
                    {
                        Description = description,
                        CreatedBy = _technicalUserId,
                        UpdatedBy = _technicalUserId,
                        CreatedAt = utcNow,
                        UpdatedAt = utcNow
                    };
                    
                    await _roleManager.CreateAsync(newRole);
                }
            }
        }
        
        /// <summary>
        /// Initializes languages from JSON file into the value list.
        /// </summary>
        private async Task InitializeLanguagesAsync()
        {
            _logger.LogInformation("Starting to initialize languages from JSON file");
            
            try
            {
                // Read languages from JSON file
                string jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "languages.json");
                
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
                    
                    jsonFilePath = Path.Combine(projectDir, "Data", "languages.json");
                }
                
                _logger.LogInformation("Reading languages from: {FilePath}", jsonFilePath);
                
                if (!File.Exists(jsonFilePath))
                {
                    _logger.LogCritical("Languages JSON file not found at: {FilePath}", jsonFilePath);
                    return;
                }
                
                string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                
                // Deserialize JSON to language objects
                var jsonLanguages = System.Text.Json.JsonSerializer.Deserialize<List<LanguageJsonModel>>(jsonContent);
                
                if (jsonLanguages == null || jsonLanguages.Count == 0)
                {
                    _logger.LogError("No languages found in JSON file or deserialization failed");
                    return;
                }
                
                _logger.LogInformation("Found {Count} languages in JSON file", jsonLanguages.Count);
                
                // Normalize and order by provided order (nulls last), then by name to keep deterministic
                var languages = jsonLanguages
                    .Select(l => new {
                        Code = l.code?.Trim() ?? string.Empty,
                        Name = l.name?.Trim() ?? string.Empty,
                        Order = l.order
                    })
                    .Where(l => !string.IsNullOrWhiteSpace(l.Code) && !string.IsNullOrWhiteSpace(l.Name))
                    .OrderBy(l => l.Order.HasValue ? 0 : 1)
                    .ThenBy(l => l.Order)
                    .ThenBy(l => l.Name)
                    .ToList();
                
                // Create or get ValueList 'Languages' and add/update items
                var languagesList = await _dbContext.ValueLists
                    .FirstOrDefaultAsync(v => v.ListName == CommonConstant.ValueListNames.Languages);
                
                if (languagesList == null)
                {
                    languagesList = new ValueList(
                        listName: CommonConstant.ValueListNames.Languages,
                        description: "List of languages",
                        valueListTypeId: ValueListConstants.ValueListTypes.Standard,
                        isActive: true,
                        allowModify: true,
                        allowNewItem: true,
                        createdBy: _technicalUserId
                    );
                    _dbContext.ValueLists.Add(languagesList);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Created ValueList 'Languages' with ID: {Id}", languagesList.Id);
                }
                
                // Fetch existing items for this list
                var existingItems = await _dbContext.ValueListItems
                    .Where(i => i.ListId == languagesList.Id)
                    .ToListAsync();
                var existingByKey = existingItems.ToDictionary(i => i.ItemKey, StringComparer.OrdinalIgnoreCase);
                
                // Prepare inserts and updates
                var newItems = new List<ValueListItem>();
                var now = DateTime.UtcNow;
                
                foreach (var l in languages)
                {
                    var item = new ValueListItem(
                        itemName: l.Name,
                        itemKey: l.Code,
                        listId: languagesList.Id,
                        isActive: true,
                        createdBy: _technicalUserId,
                        order: l.Order
                    );
                    newItems.Add(item);
                }
                
                if (newItems.Count > 0)
                {
                    await _dbContext.ValueListItems.AddRangeAsync(newItems);
                    _logger.LogInformation("Prepared {Count} new languages to insert", newItems.Count);
                }
                
                // Save all changes if any
                if (newItems.Count > 0)
                {
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Languages initialization completed with inserts: {Inserts}", newItems.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to initialize languages");
            }
        }

        /// <summary>
        /// Seeds default permissions into the system.
        /// </summary>
        private async Task SeedDefaultPermissionsAsync()
        {
            var utcNow = DateTime.UtcNow;
            
            foreach (var permissionName in OXDesk.Core.Common.CommonConstant.PermissionNames.All)
            {
                // Check if permission already exists by normalized name (uppercase)
                var normalizedName = permissionName.ToUpperInvariant();
                var existingPermission = await _permissionRepository.GetByNameAsync(normalizedName);
                
                if (existingPermission == null)
                {
                    // Create new permission
                    var permission = new OXDesk.Core.Identity.Permission
                    {
                        Name = permissionName,
                        NormalizedName = normalizedName,
                        CreatedBy = _technicalUserId,
                        CreatedAt = utcNow,
                        UpdatedBy = _technicalUserId,
                        UpdatedAt = utcNow
                    };
                    
                    await _permissionRepository.AddAsync(permission);
                }
            }
        }

        /// <summary>
        /// Creates the technical system user for internal operations.
        /// </summary>
        private async Task CreateSystemUserAsync()
        {
            // Check if system user already exists
            var systemUserEmail = CommonConstant.TECHNICAL_USERNAME;
            var existingUser = await _userManager.FindByEmailAsync(systemUserEmail);
            
            if (existingUser == null)
            {
                // Set profile key for System user directly using the value list item key
                string? systemProfileKey = CommonConstant.UserProfiles.System.Key;
                var utcNow = DateTime.UtcNow;
                
                // Create the system user
                var systemUser = new OXDesk.Core.Identity.ApplicationUser
                {
                    TenantId = _tenantId,
                    UserName = systemUserEmail,
                    Email = systemUserEmail,
                    FirstName = "OX Desk",
                    LastName = "Interface",
                    Location = "Earth",
                    Profile = systemProfileKey,
                    CountryCode = "IN",
                    Language = "en-US",
                    TimeZone = "UTC",
                    IsActive = "N", // Inactive user
                    EmailConfirmed = true,
                    CreatedAt = utcNow,
                    CreatedBy = _technicalUserId,
                    UpdatedAt = utcNow,
                    UpdatedBy = _technicalUserId
                };
                
                // Generate a strong random password
                var password = Guid.CreateVersion7().ToString() + "!Aa1";
                
                // Create the user
                var result = await _userManager.CreateAsync(systemUser, password);
                
                if (result.Succeeded)
                {
                    // Instead of using UserManager.AddToRoleAsync, we'll create the user role directly

                    // Add change history log for creation (first change event)
                    await _changeHistoryLogService.CreateChangeHistoryLogAsync(
                        objectKey: DynamicObjectConstants.DynamicObjectKeys.User,
                        objectItemId: systemUser.Id,
                        fieldName: CommonConstant.ChangeHistoryFields.Created,
                        oldValue: null,
                        newValue: null,
                        createdBy: _technicalUserId,
                        cancellationToken: default
                    );
                    
                    // Get the role ID
                    var role = await _roleManager.FindByNameAsync(OXDesk.Core.Common.CommonConstant.RoleNames.System);
                    if (role == null)
                    {
                        _logger.LogCritical("Could not find System role when assigning to system user");
                        return;
                    }
                    
                    // Create a new ApplicationUserRole with audit fields
                    var userRole = new OXDesk.Core.Identity.ApplicationUserRole
                    {
                        UserId = systemUser.Id,
                        RoleId = role.Id,
                        CreatedBy = _technicalUserId,
                        CreatedAt = utcNow,
                        UpdatedBy = _technicalUserId,
                        UpdatedAt = utcNow
                    };
                    
                    try
                    {
                        // Add the user role directly to the database
                        _dbContext.UserRoles.Add(userRole);
                        await _dbContext.SaveChangesAsync();
                        
                        _logger.LogInformation("Added system user to System role with audit fields");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "Failed to add system user to System role");
                    }
                }
                else
                {
                _logger.LogInformation("System user already exists with ID: {Id}", existingUser?.Id);
                }
            }
        }
        
        /// <summary>
        /// Sets up the initial tenant and admin user account using AppSetupService.
        /// </summary>
        private async Task SetupTenantAndAdminAsync()
        {
            try
            {
                // Skip if already installed
                if (await _appSetupService.IsApplicationInstalledAsync())
                {
                    _logger.LogInformation("Application already installed. Skipping 'Setup Tenant and Admin User' step.");
                    return;
                }

                var setupDto = new AppSetupDto
                {
                    TenantName = "NP Photography",
                    Host = "OXDESK.LOCAL",
                    FirstName = "Nithin",
                    LastName = "Prathapan",
                    AdminEmail = "nithinp89@gmail.com",
                    Password = "Admin@123$",
                    ConfirmPassword = "Admin@123$",
                    TimeZone = "UTC",
                    Location = "Kerala",
                    CountryCode = "IN"
                };

                _logger.LogInformation("Installing application for tenant '{TenantName}' on host '{Host}' with tenant ID: {TenantId}", setupDto.TenantName, setupDto.Host, _tenantId);
                var tenant = await _appSetupService.InstallApplicationAsync(setupDto, _tenantId, _technicalUserId);
                _logger.LogInformation("Application installed for tenant {TenantId} - {TenantName}", tenant.Id, setupDto.TenantName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed during 'Setup Tenant and Admin User' step");
                throw;
            }
        }

        /// <summary>
        /// Defines and creates dynamic objects in the system.
        /// </summary>
        private async Task DefineDynamicObjectsAsync()
        {
            _logger.LogInformation("Starting to define dynamic objects");
            
            // Define the dynamic objects with their properties
            var dynamicObjects = new List<OXDesk.Core.DynamicObjects.DynamicObject>
            {
                // User dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.User,
                    "User",
                    "Users",
                    DynamicObjectConstants.DynamicObjectKeys.User,
                    "System user entity for authentication and authorization",
                    DynamicObjectConstants.ObjectTypes.Dedicated,
                    _technicalUserId
                ),

                // Role dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Role,
                    "Role",
                    "Roles",
                    DynamicObjectConstants.DynamicObjectKeys.Role,
                    "System user roles entity",
                    DynamicObjectConstants.ObjectTypes.Dedicated,
                    _technicalUserId
                ),

                // Permission dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Permission,
                    "Permission",
                    "Permissions",
                    DynamicObjectConstants.DynamicObjectKeys.Permission,
                    "System user permissions entity",
                    DynamicObjectConstants.ObjectTypes.Dedicated,
                    _technicalUserId
                ),
                
                // Account dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Account,
                    "Account",
                    "Accounts",
                    DynamicObjectConstants.DynamicObjectKeys.Account,
                    "Business or organization account",
                    DynamicObjectConstants.ObjectTypes.Dedicated,
                    _technicalUserId
                ),
                
                // Contact dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Contact,
                    "Contact",
                    "Contacts",
                    DynamicObjectConstants.DynamicObjectKeys.Contact,
                    "Individual contact associated with accounts",
                    DynamicObjectConstants.ObjectTypes.Dedicated,
                    _technicalUserId
                ),
                
                // Ticket dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Ticket,
                    "Ticket",
                    "Tickets",
                    DynamicObjectConstants.DynamicObjectKeys.Ticket,
                    "Support or service ticket",
                    DynamicObjectConstants.ObjectTypes.Dedicated,
                    _technicalUserId
                )
                ,
                // Tenant dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Tenant,
                    "Tenant",
                    "Tenants",
                    DynamicObjectConstants.DynamicObjectKeys.Tenant,
                    "Tenant entity representing a customer account",
                    DynamicObjectConstants.ObjectTypes.System,
                    _technicalUserId
                )
                ,
                // Brand dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Brand,
                    "Brand",
                    "Brands",
                    DynamicObjectConstants.DynamicObjectKeys.Brand,
                    "Brand entity",
                    DynamicObjectConstants.ObjectTypes.System,
                    _technicalUserId
                )
                ,
                // Entity dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Entity,
                    "Entity",
                    "Entities",
                    DynamicObjectConstants.DynamicObjectKeys.Entity,
                    "Entity",
                    DynamicObjectConstants.ObjectTypes.System,
                    _technicalUserId
                )
                ,
                // Group dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Group,
                    "Group",
                    "Groups",
                    DynamicObjectConstants.DynamicObjectKeys.Group,
                    "Group",
                    DynamicObjectConstants.ObjectTypes.System,
                    _technicalUserId
                )
                ,
                // Product dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Product,
                    "Product",
                    "Products",
                    DynamicObjectConstants.DynamicObjectKeys.Product,
                    "Product entity",
                    DynamicObjectConstants.ObjectTypes.Standard,
                    _technicalUserId
                )
                ,
                // Business hours dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.BusinessHours,
                    "BusinessHour",
                    "BusinessHours",
                    DynamicObjectConstants.DynamicObjectKeys.BusinessHours,
                    "Business hours",
                    DynamicObjectConstants.ObjectTypes.System,
                    _technicalUserId
                )
                ,
                // Signatures dynamic object
                new DynamicObject(
                    DynamicObjectConstants.DynamicObjectNames.Signatures,
                    "Signature",
                    "Signatures",
                    DynamicObjectConstants.DynamicObjectKeys.Signatures,
                    "Signature",
                    DynamicObjectConstants.ObjectTypes.System,
                    _technicalUserId
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
        
        /// <summary>
        /// Initializes countries from JSON file into the value list.
        /// </summary>
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
                var countries = jsonCountries.Select(c => new {
                    CountryName = c.Country_Name?.Trim() ?? string.Empty,
                    CountryCode = c.Country_Code?.Trim() ?? string.Empty,
                    CountryCodeAlpha3 = c.Country_Code_Alpha_3?.Trim() ?? string.Empty,
                    IsActive = c.Active?.Trim() ?? "Y"
                }).OrderBy(c => c.CountryName).ToList();

                // Create or get ValueList 'Countries' and add items (Country Code, Country Name)
                var countriesList = await _dbContext.ValueLists
                    .FirstOrDefaultAsync(v => v.ListName == CommonConstant.ValueListNames.Countries);

                if (countriesList == null)
                {
                    countriesList = new ValueList(
                        listName: CommonConstant.ValueListNames.Countries,
                        description: "List of countries",
                        valueListTypeId: ValueListConstants.ValueListTypes.Standard,
                        isActive: true,
                        allowModify: false,
                        allowNewItem: false,
                        createdBy: _technicalUserId
                    );
                    _dbContext.ValueLists.Add(countriesList);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Created ValueList 'Countries' with ID: {Id}", countriesList.Id);
                }

                // Existing item keys (country codes) for this list
                var existingItemKeys = await _dbContext.ValueListItems
                    .Where(i => i.ListId == countriesList.Id)
                    .Select(i => i.ItemKey)
                    .ToListAsync();

                
                var newValueListItems = countries
                    .Where(c => !string.IsNullOrWhiteSpace(c.CountryCode))
                    .Where(c => !existingItemKeys.Contains(c.CountryCode))
                    .Select(c => new ValueListItem(
                        itemName: c.CountryName,
                        itemKey: c.CountryCode,
                        listId: countriesList.Id,
                        isActive: string.Equals(c.IsActive, "Y", StringComparison.OrdinalIgnoreCase),
                        createdBy: _technicalUserId
                    ))
                    .ToList();

                if (newValueListItems.Count > 0)
                {
                    await _dbContext.ValueListItems.AddRangeAsync(newValueListItems);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Added {Count} countries to ValueList 'Countries' as items", newValueListItems.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,"Failed to initialize countries");
            }
        }
        
        /// <summary>
        /// Initializes currencies from JSON file into the value list.
        /// </summary>
        private async Task InitializeCurrenciesAsync()
        {
            _logger.LogInformation("Starting to initialize currencies from JSON file");
            
            try
            {
                // Read currencies from JSON file
                string jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "currencies.json");
                
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
                    
                    jsonFilePath = Path.Combine(projectDir, "Data", "currencies.json");
                }
                
                _logger.LogInformation("Reading currencies from: {FilePath}", jsonFilePath);
                
                if (!File.Exists(jsonFilePath))
                {
                    _logger.LogCritical("Currencies JSON file not found at: {FilePath}", jsonFilePath);
                    return;
                }
                
                string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                
                // Deserialize JSON to currency objects
                var jsonCurrencies = System.Text.Json.JsonSerializer.Deserialize<List<CurrencyJsonModel>>(jsonContent);
                
                if (jsonCurrencies == null || jsonCurrencies.Count == 0)
                {
                    _logger.LogError("No currencies found in JSON file or deserialization failed");
                    return;
                }
                
                _logger.LogInformation("Found {Count} currencies in JSON file", jsonCurrencies.Count);
                
                // Normalize and order
                var currencies = jsonCurrencies
                    .Select(c => new {
                        Code = c.code?.Trim() ?? string.Empty,
                        Name = c.name?.Trim() ?? string.Empty
                    })
                    .Where(c => !string.IsNullOrWhiteSpace(c.Code) && !string.IsNullOrWhiteSpace(c.Name))
                    .OrderBy(c => c.Name)
                    .ToList();
                
                // Create or get ValueList 'Currencies' and add items (Code, Name)
                var currenciesList = await _dbContext.ValueLists
                    .FirstOrDefaultAsync(v => v.ListName == "Currencies");
                
                if (currenciesList == null)
                {
                    currenciesList = new ValueList(
                        listName: CommonConstant.ValueListNames.Currencies,
                        description: "List of currencies",
                        valueListTypeId: ValueListConstants.ValueListTypes.Standard,
                        isActive: true,
                        allowModify: false,
                        allowNewItem: false,
                        createdBy: _technicalUserId
                    );
                    _dbContext.ValueLists.Add(currenciesList);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Created ValueList 'Currencies' with ID: {Id}", currenciesList.Id);
                }
                
                // Existing item keys (currency codes) for this list
                var existingCurrencyKeys = await _dbContext.ValueListItems
                    .Where(i => i.ListId == currenciesList.Id)
                    .Select(i => i.ItemKey)
                    .ToListAsync();
                
                
                var newValueListItems = currencies
                    .Where(c => !existingCurrencyKeys.Contains(c.Code))
                    .Select(c => new ValueListItem(
                        itemName: c.Name,
                        itemKey: c.Code,
                        listId: currenciesList.Id,
                        isActive: true,
                        createdBy: _technicalUserId
                    ))
                    .ToList();
                
                if (newValueListItems.Count > 0)
                {
                    await _dbContext.ValueListItems.AddRangeAsync(newValueListItems);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Added {Count} currencies to ValueList 'Currencies' as items", newValueListItems.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to initialize currencies");
            }
        }
        
        /// <summary>
        /// Initializes user profiles into the value list.
        /// </summary>
        private async Task InitializeUserProfile()
        {
            _logger.LogInformation("Starting to initialize user profiles");
            
            try
            {
                // Create or get ValueList 'User Profiles'
                var profilesList = await _dbContext.ValueLists
                    .FirstOrDefaultAsync(v => v.ListName == CommonConstant.ValueListNames.UserProfiles);
                
                if (profilesList == null)
                {
                    profilesList = new ValueList(
                        listName: CommonConstant.ValueListNames.UserProfiles,
                        description: "List of user profiles",
                        valueListTypeId: ValueListConstants.ValueListTypes.Standard,
                        isActive: true,
                        allowModify: false,
                        allowNewItem: false,
                        createdBy: _technicalUserId
                    );
                    _dbContext.ValueLists.Add(profilesList);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Created ValueList 'User Profiles' with ID: {Id}", profilesList.Id);
                }
                
                // Existing item values for this list
                var existingProfileKeys = await _dbContext.ValueListItems
                    .Where(i => i.ListId == profilesList.Id)
                    .Select(i => i.ItemKey)
                    .ToListAsync();
                
                var desiredProfiles = CommonConstant.UserProfiles.All;
                
                
                var newItems = desiredProfiles
                    .Where(p => !existingProfileKeys.Contains(p.Key))
                    .Select(p => new ValueListItem(
                        itemName: p.Name,
                        itemKey: p.Key,
                        listId: profilesList.Id,
                        isActive: true,
                        createdBy: _technicalUserId
                    ))
                    .ToList();
                
                if (newItems.Count > 0)
                {
                    await _dbContext.ValueListItems.AddRangeAsync(newItems);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Added {Count} user profiles to ValueList 'User Profiles' as items", newItems.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to initialize user profiles");
            }
        }
        
        /// <summary>
        /// Initializes dynamic object field types.
        /// </summary>
        private async Task InitializeFieldTypesAsync()
        {
            _logger.LogInformation("Starting to initialize dynamic object field types");
            
            try
            {
                var now = DateTime.UtcNow;
                
                // Define desired field types with recommended configs
                var desired = new List<DynamicObjectFieldType>
                {
                    new()
                    {
                        Name = "Auto Number", FieldTypeKey = "auto-number",
                        Description = "Automatically incrementing number",
                        Decimals = 0,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Text (Single Line)", FieldTypeKey = "text",
                        Description = "Single-line text",
                        MaxLength = 255,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Text Area (Multi-line)", FieldTypeKey = "textarea",
                        Description = "Multi-line text",
                        MaxLength = 2000,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Text Area (Large)", FieldTypeKey = "textarea-large",
                        Description = "Large multi-line text",
                        MaxLength = 32768,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Text Area (Rich/Html)", FieldTypeKey = "textarea-rich",
                        Description = "Rich text / HTML",
                        MaxLength = 65536,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Number (integer)", FieldTypeKey = "number-int",
                        Description = "Integer number",
                        Decimals = 0,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Number Decimal", FieldTypeKey = "number-decimal",
                        Description = "Decimal number",
                        Decimals = 2,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Currency", FieldTypeKey = "currency",
                        Description = "Currency with code",
                        Decimals = 2,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Percent", FieldTypeKey = "percent",
                        Description = "Percentage value",
                        Decimals = 2,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Date", FieldTypeKey = "date",
                        Description = "Date picker",
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Date & Time", FieldTypeKey = "date-time",
                        Description = "Date and time picker",
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Email", FieldTypeKey = "email",
                        Description = "Email address",
                        MaxLength = 255,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Phone", FieldTypeKey = "phone",
                        Description = "Phone number",
                        MaxLength = 30,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Link", FieldTypeKey = "link",
                        Description = "URL / hyperlink",
                        MaxLength = 2048,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Credit Card Number", FieldTypeKey = "credit-card",
                        Description = "Credit card number",
                        MaxLength = 19,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "File", FieldTypeKey = "file",
                        Description = "File upload",
                        MaxFileSize = 25,
                        AllowedFileTypes = "pdf,doc,docx,xls,xlsx,ppt,pptx,txt,csv,zip,jpg,jpeg,png",
                        MinFileCount = 0,
                        MaxFileCount = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Image", FieldTypeKey = "image",
                        Description = "Image upload",
                        MaxFileSize = 10,
                        AllowedFileTypes = "jpg,jpeg,png",
                        MinFileCount = 0,
                        MaxFileCount = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Dropdown (single select)", FieldTypeKey = "dropdown-single",
                        Description = "Dropdown with single selection",
                        MinSelectedItems = 0, MaxSelectedItems = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Dropdown (multi select)", FieldTypeKey = "dropdown-multi",
                        Description = "Dropdown with multiple selection",
                        MinSelectedItems = 0,
                        MaxSelectedItems = 50,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Checkbox (multi)", FieldTypeKey = "checkbox-multi",
                        Description = "Multiple checkboxes",
                        MinSelectedItems = 0,
                        MaxSelectedItems = 50,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Checkbox (Boolean)", FieldTypeKey = "checkbox-boolean",
                        Description = "Single checkbox (true/false)",
                        MinSelectedItems = 0,
                        MaxSelectedItems = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                    new()
                    {
                        Name = "Radio / varchar(255)", FieldTypeKey = "radio",
                        Description = "Radio button (single choice)",
                        MinSelectedItems = 0,
                        MaxSelectedItems = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = _technicalUserId, UpdatedBy = _technicalUserId
                    },
                };
                
                var existingKeys = await _dbContext.DynamicObjectFieldTypes
                    .AsNoTracking()
                    .Select(x => x.FieldTypeKey)
                    .ToListAsync();
                
                var toInsert = desired
                    .Where(d => !existingKeys.Contains(d.FieldTypeKey))
                    .ToList();
                
                if (toInsert.Count > 0)
                {
                    await _dbContext.DynamicObjectFieldTypes.AddRangeAsync(toInsert);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Added {Count} dynamic object field types", toInsert.Count);
                }
                else
                {
                    _logger.LogInformation("No new dynamic object field types to add");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to initialize dynamic object field types");
            }
        }
        
        /// <summary>
        /// Initializes request types into the value list.
        /// </summary>
        private async Task InitializeRequestTypesAsync()
        {
            _logger.LogInformation("Starting to initialize request types");
            
            try
            {
                // Create or get ValueList 'Request Types'
                var requestTypesList = await _dbContext.ValueLists
                    .FirstOrDefaultAsync(v => v.ListName == CommonConstant.ValueListNames.RequestTypes);
                
                if (requestTypesList == null)
                {
                    
                    requestTypesList = new ValueList(
                        listName: CommonConstant.ValueListNames.RequestTypes,
                        description: "List of request types",
                        valueListTypeId: ValueListConstants.ValueListTypes.Standard,
                        isActive: true,
                        allowModify: false,
                        allowNewItem: false,
                        createdBy: _technicalUserId
                    );
                    _dbContext.ValueLists.Add(requestTypesList);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Created ValueList 'Request Types' with ID: {Id}", requestTypesList.Id);
                }
                
                // Existing item keys for this list
                var existingKeys = await _dbContext.ValueListItems
                    .Where(i => i.ListId == requestTypesList.Id)
                    .Select(i => i.ItemKey)
                    .ToListAsync();
                
                var desired = new (string Name, string Key)[]
                {
                    ("Feature", "feature"),
                    ("Problem", "problem"),
                    ("Question", "question"),
                    ("Others", "others")
                };
                
                // Get default tenant ID for system initialization
                
                
                var newItems = desired
                    .Where(d => !existingKeys.Contains(d.Key))
                    .Select(d => new ValueListItem(
                        itemName: d.Name,
                        itemKey: d.Key,
                        listId: requestTypesList.Id,
                        isActive: true,
                        createdBy: _technicalUserId
                    ))
                    .ToList();
                
                if (newItems.Count > 0)
                {
                    await _dbContext.ValueListItems.AddRangeAsync(newItems);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Added {Count} request types to ValueList 'Request Types'", newItems.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to initialize request types");
            }
        }
        
        
        /// <summary>
        /// Gets the default tenant ID for system initialization.
        /// </summary>
        /// <returns>The default tenant ID.</returns>
        private Task<Guid> Get_tenantIdAsync()
        {
            // Use a fixed system tenant ID for initialization
            // This ensures all seeded entities belong to the same tenant
            return Task.FromResult(_tenantId);
        }
        
        /// <summary>
        /// Model class for deserializing country data from JSON.
        /// </summary>
        private sealed class CountryJsonModel
        {
            public string? Country_Name { get; init; }
            public string? Country_Code { get; init; }
            public string? Country_Code_Alpha_3 { get; init; }
            public string? Active { get; init; }
        }
        
        /// <summary>
        /// Model class for deserializing currency data from JSON.
        /// </summary>
        private sealed class CurrencyJsonModel
        {
            public string? code { get; init; }
            public string? name { get; init; }
        }
        
        /// <summary>
        /// Model class for deserializing language data from JSON.
        /// </summary>
        private sealed class LanguageJsonModel
        {
            public int? order { get; init; }
            public string? code { get; init; }
            public string? name { get; init; }
        }
        
        /// <summary>
        /// Assigns permissions to roles based on predefined mappings.
        /// </summary>
        private async Task AssignPermissionsToRolesAsync()
        {
            // Define role-based permissions
            var rolePermissions = new Dictionary<string, string[]>
            {
                // System role has all permissions
                [OXDesk.Core.Common.CommonConstant.RoleNames.System] = OXDesk.Core.Common.CommonConstant.PermissionNames.All,
                
                // Administrator has all permissions
                [OXDesk.Core.Common.CommonConstant.RoleNames.Administrator] = OXDesk.Core.Common.CommonConstant.PermissionNames.All,
                
                // Power User has most permissions except system update setup
                [OXDesk.Core.Common.CommonConstant.RoleNames.PowerUser] = 
                [
                    OXDesk.Core.Common.CommonConstant.PermissionNames.SysSetupRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.UserRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TicketRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TicketWrite,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.ContactRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.ContactWrite,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.AccountRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.AccountWrite,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TemplateRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TemplateWrite
                ],
                
                // Support Agent has ticket and contact permissions
                [OXDesk.Core.Common.CommonConstant.RoleNames.SupportAgent] = 
                [
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TicketRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TicketWrite,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.ContactRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.ContactWrite,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.AccountRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.AccountWrite,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TemplateRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TemplateWrite
                ],
                
                // Light Agent has read-only permissions
                [OXDesk.Core.Common.CommonConstant.RoleNames.LightAgent] = 
                [
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TicketRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.ContactRead,
                    OXDesk.Core.Common.CommonConstant.PermissionNames.AccountRead
                ],
                
                // End User has minimal permissions
                [OXDesk.Core.Common.CommonConstant.RoleNames.EndUser] = 
                [
                    OXDesk.Core.Common.CommonConstant.PermissionNames.TicketRead
                ]
            };
            
            // Get all permissions from the repository after ensuring they're seeded (global filters apply)
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
                        var utcNow = DateTime.UtcNow;
                        var normalizedName = permissionName.ToUpperInvariant();
                        
                        permission = new OXDesk.Core.Identity.Permission
                        {
                            Name = permissionName,
                            NormalizedName = normalizedName,
                            CreatedBy = _technicalUserId,
                            CreatedAt = utcNow,
                            UpdatedBy = _technicalUserId,
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
                        var utcNow = DateTime.UtcNow;
                        
                        // Create a new ApplicationRoleClaim with audit fields
                        var roleClaim = new OXDesk.Core.Identity.ApplicationRoleClaim
                        {
                            RoleId = role.Id,
                            ClaimType = claimType,
                            ClaimValue = claimValue,
                            CreatedBy = _technicalUserId,
                            CreatedAt = utcNow,
                            UpdatedBy = _technicalUserId,
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
