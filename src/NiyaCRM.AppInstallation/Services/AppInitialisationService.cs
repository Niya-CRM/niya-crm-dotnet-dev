using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NiyaCRM.Core.AppInstallation;
using NiyaCRM.Core.AppInstallation.AppInitialisation;
using NiyaCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

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
            ("Initial", "0.0.1", 1, "Seed Default Roles", async () => {
                await SeedDefaultRolesAsync();
            }),
            ("Initial", "0.0.1", 2, "Seed Default Permissions", async () => {
                await SeedDefaultPermissionsAsync();
            }),
            ("Initial", "0.0.1", 3, "Create Technical User", async () => {
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

        public async Task InitialiseAppAsync()
        {
            // Check if any status records exist
            if (!await _dbContext.AppInstallationStatus.AnyAsync())
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
                await _dbContext.SaveChangesAsync();
            }

            // Run each step that is not completed
            var statuses = _dbContext.AppInstallationStatus.Where(s => s.Completed != "Y").ToList();
            foreach (var status in statuses)
            {
                var step = Steps.FirstOrDefault(x => x.Pipeline == status.Pipeline && x.Version == status.Version && x.Order == status.Order && x.Step == status.Step);
                if (step.Action != null)
                {
                    await step.Action();
                    status.Completed = "Y";
                }
            }
            await _dbContext.SaveChangesAsync();
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
                    TimeZone = "UTC",
                    IsActive = "N", // Inactive user
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = NiyaCRM.Core.Common.CommonConstant.DEFAULT_TECHNICAL_USER
                };
                
                // Generate a strong random password
                var password = Guid.CreateVersion7().ToString() + "!Aa1";
                
                // Create the user
                var result = await _userManager.CreateAsync(technicalUser, password);
                
                if (result.Succeeded)
                {
                    // Add user to Administrator role
                    await _userManager.AddToRoleAsync(technicalUser, NiyaCRM.Core.Common.CommonConstant.RoleNames.Administrator);
                }
                else
                {
                    // Log the error
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create technical user: {Errors}", errors);
                }
            }
        }
    }
}
