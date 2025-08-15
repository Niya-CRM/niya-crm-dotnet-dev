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
using NiyaCRM.Core.DynamicObjects.Fields;
using NiyaCRM.Infrastructure.Data;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.ValueLists;

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
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 8, "Initialize Field Types", async () => {
                await InitializeFieldTypesAsync();
            }),
            (CommonConstant.AppInstallation.Pipeline.Initial, CommonConstant.AppInstallation.INITIAL_VERSION, 9, "Create Technical User", async () => {
                await CreateTechnicalUserAsync();
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

        /// <summary>
        /// Create Technical User
        /// </summary>
        /// <returns></returns>

        private async Task CreateTechnicalUserAsync()
        {
            // Check if technical user already exists
            var technicalUserEmail = "NiyaCRM@system.local";
            var existingUser = await _userManager.FindByEmailAsync(technicalUserEmail);
            
            if (existingUser == null)
            {
                // Resolve profile GUID for "Technical" from ValueList "User Profiles"
                Guid? technicalProfileId = null;
                try
                {
                    var profilesListId = await _dbContext.ValueLists
                        .Where(v => v.Name == "User Profiles")
                        .Select(v => (Guid?)v.Id)
                        .FirstOrDefaultAsync();

                    if (profilesListId.HasValue)
                    {
                        technicalProfileId = await _dbContext.ValueListItems
                            .Where(i => i.ValueListId == profilesListId.Value && i.ItemValue == "Technical")
                            .Select(i => (Guid?)i.Id)
                            .FirstOrDefaultAsync();
                    }

                    if (!technicalProfileId.HasValue)
                    {
                        _logger.LogWarning("Technical profile not found in 'User Profiles'; creating technical user without profile.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve 'Technical' profile from 'User Profiles'.");
                }
                
                // Create the technical user
                var technicalUser = new NiyaCRM.Core.Identity.ApplicationUser
                {
                    Id = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER,
                    UserName = technicalUserEmail,
                    Email = technicalUserEmail,
                    FirstName = "Technical",
                    LastName = "Interface",
                    Location = "Earth",
                    Profile = technicalProfileId,
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
                var countries = jsonCountries.Select(c => new {
                    CountryName = c.Country_Name?.Trim() ?? string.Empty,
                    CountryCode = c.Country_Code?.Trim() ?? string.Empty,
                    CountryCodeAlpha3 = c.Country_Code_Alpha_3?.Trim() ?? string.Empty,
                    IsActive = c.Active?.Trim() ?? "Y"
                }).OrderBy(c => c.CountryName).ToList();

                // Create or get ValueList 'Countries' and add items (Country Code, Country Name)
                var defaultUser = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER;

                var countriesList = await _dbContext.ValueLists
                    .FirstOrDefaultAsync(v => v.Name == "Countries");

                if (countriesList == null)
                {
                    countriesList = new ValueList(
                        Guid.CreateVersion7(),
                        name: "Countries",
                        description: "List of countries",
                        valueListTypeId: ValueListConstants.ValueListTypes.Standard,
                        isActive: true,
                        allowModify: true,
                        allowNewItem: true,
                        createdBy: defaultUser
                    );
                    _dbContext.ValueLists.Add(countriesList);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Created ValueList 'Countries' with ID: {Id}", countriesList.Id);
                }

                // Existing item values (country codes) for this list
                var existingItemValues = await _dbContext.ValueListItems
                    .Where(i => i.ValueListId == countriesList.Id)
                    .Select(i => i.ItemValue)
                    .ToListAsync();

                var newValueListItems = countries
                    .Where(c => !string.IsNullOrWhiteSpace(c.CountryCode))
                    .Where(c => !existingItemValues.Contains(c.CountryCode))
                    .Select(c => new ValueListItem(
                        Guid.CreateVersion7(),
                        itemName: c.CountryName,
                        itemValue: c.CountryCode,
                        valueListId: countriesList.Id,
                        isActive: string.Equals(c.IsActive, "Y", StringComparison.OrdinalIgnoreCase),
                        createdBy: defaultUser
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
                var defaultUser = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER;
                
                var currenciesList = await _dbContext.ValueLists
                    .FirstOrDefaultAsync(v => v.Name == "Currencies");
                
                if (currenciesList == null)
                {
                    currenciesList = new ValueList(
                        Guid.CreateVersion7(),
                        name: "Currencies",
                        description: "List of currencies",
                        valueListTypeId: ValueListConstants.ValueListTypes.Standard,
                        isActive: true,
                        allowModify: true,
                        allowNewItem: true,
                        createdBy: defaultUser
                    );
                    _dbContext.ValueLists.Add(currenciesList);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Created ValueList 'Currencies' with ID: {Id}", currenciesList.Id);
                }
                
                // Existing item values (currency codes) for this list
                var existingItemValues = await _dbContext.ValueListItems
                    .Where(i => i.ValueListId == currenciesList.Id)
                    .Select(i => i.ItemValue)
                    .ToListAsync();
                
                var newValueListItems = currencies
                    .Where(c => !existingItemValues.Contains(c.Code))
                    .Select(c => new ValueListItem(
                        Guid.CreateVersion7(),
                        itemName: c.Name,
                        itemValue: c.Code,
                        valueListId: currenciesList.Id,
                        isActive: true,
                        createdBy: defaultUser
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
        
        private async Task InitializeUserProfile()
        {
            _logger.LogInformation("Starting to initialize user profiles");
            
            try
            {
                var defaultUser = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER;
                
                // Create or get ValueList 'User Profiles'
                var profilesList = await _dbContext.ValueLists
                    .FirstOrDefaultAsync(v => v.Name == "User Profiles");
                
                if (profilesList == null)
                {
                    profilesList = new ValueList(
                        Guid.CreateVersion7(),
                        name: "User Profiles",
                        description: "List of user profiles",
                        valueListTypeId: ValueListConstants.ValueListTypes.Standard,
                        isActive: true,
                        allowModify: false,
                        allowNewItem: false,
                        createdBy: defaultUser
                    );
                    _dbContext.ValueLists.Add(profilesList);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Created ValueList 'User Profiles' with ID: {Id}", profilesList.Id);
                }
                
                // Existing item values for this list
                var existingItemValues = await _dbContext.ValueListItems
                    .Where(i => i.ValueListId == profilesList.Id)
                    .Select(i => i.ItemValue)
                    .ToListAsync();
                
                var desiredProfiles = new[] { "Administrator", "Agent", "Light Agent", "External User", "Technical" };
                
                var newItems = desiredProfiles
                    .Where(name => !existingItemValues.Contains(name))
                    .Select(name => new ValueListItem(
                        Guid.CreateVersion7(),
                        itemName: name,
                        itemValue: name,
                        valueListId: profilesList.Id,
                        isActive: true,
                        createdBy: defaultUser
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
        
        private async Task InitializeFieldTypesAsync()
        {
            _logger.LogInformation("Starting to initialize dynamic object field types");
            
            try
            {
                var defaultUser = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER;
                var now = DateTime.UtcNow;
                
                // Define desired field types with recommended configs
                var desired = new List<DynamicObjectFieldType>
                {
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Auto Number", FieldTypeKey = "auto-number",
                        Description = "Automatically incrementing number",
                        Decimals = 0,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Text (Single Line)", FieldTypeKey = "text",
                        Description = "Single-line text",
                        MaxLength = 255,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Text Area (Multi-line)", FieldTypeKey = "textarea",
                        Description = "Multi-line text",
                        MaxLength = 2000,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Text Area (Large)", FieldTypeKey = "textarea-large",
                        Description = "Large multi-line text",
                        MaxLength = 32768,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Text Area (Rich/Html)", FieldTypeKey = "textarea-rich",
                        Description = "Rich text / HTML",
                        MaxLength = 65536,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Number (integer)", FieldTypeKey = "number-int",
                        Description = "Integer number",
                        Decimals = 0,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Number Decimal", FieldTypeKey = "number-decimal",
                        Description = "Decimal number",
                        Decimals = 2,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Currency", FieldTypeKey = "currency",
                        Description = "Currency with code",
                        Decimals = 2,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Percent", FieldTypeKey = "percent",
                        Description = "Percentage value",
                        Decimals = 2,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Date", FieldTypeKey = "date",
                        Description = "Date picker",
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Date & Time", FieldTypeKey = "date-time",
                        Description = "Date and time picker",
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Email", FieldTypeKey = "email",
                        Description = "Email address",
                        MaxLength = 255,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Phone", FieldTypeKey = "phone",
                        Description = "Phone number",
                        MaxLength = 30,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Link", FieldTypeKey = "link",
                        Description = "URL / hyperlink",
                        MaxLength = 2048,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Credit Card Number", FieldTypeKey = "credit-card",
                        Description = "Credit card number",
                        MaxLength = 19,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "File", FieldTypeKey = "file",
                        Description = "File upload",
                        MaxFileSize = 25,
                        AllowedFileTypes = "pdf,doc,docx,xls,xlsx,ppt,pptx,txt,csv,zip,jpg,jpeg,png",
                        MinFileCount = 0,
                        MaxFileCount = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Image", FieldTypeKey = "image",
                        Description = "Image upload",
                        MaxFileSize = 10,
                        AllowedFileTypes = "jpg,jpeg,png",
                        MinFileCount = 0,
                        MaxFileCount = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Dropdown (single select)", FieldTypeKey = "dropdown-single",
                        Description = "Dropdown with single selection",
                        MinSelectedItems = 0, MaxSelectedItems = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Dropdown (multi select)", FieldTypeKey = "dropdown-multi",
                        Description = "Dropdown with multiple selection",
                        MinSelectedItems = 0,
                        MaxSelectedItems = 50,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Checkbox (multi)", FieldTypeKey = "checkbox-multi",
                        Description = "Multiple checkboxes",
                        MinSelectedItems = 0,
                        MaxSelectedItems = 50,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Checkbox (Boolean)", FieldTypeKey = "checkbox-boolean",
                        Description = "Single checkbox (true/false)",
                        MinSelectedItems = 0,
                        MaxSelectedItems = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7(), Name = "Radio / varchar(255)", FieldTypeKey = "radio",
                        Description = "Radio button (single choice)",
                        MinSelectedItems = 0,
                        MaxSelectedItems = 1,
                        CreatedAt = now, UpdatedAt = now, CreatedBy = defaultUser, UpdatedBy = defaultUser
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
        
        // Model class for deserializing the JSON country data
        private sealed class CountryJsonModel
        {
            public string? Country_Name { get; init; }
            public string? Country_Code { get; init; }
            public string? Country_Code_Alpha_3 { get; init; }
            public string? Active { get; init; }
        }
        
        // Model class for deserializing the JSON currency data
        private sealed class CurrencyJsonModel
        {
            public string? code { get; init; }
            public string? name { get; init; }
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
