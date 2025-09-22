using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
 

namespace OXDesk.Application.Identity
{
    public sealed class RoleService : IRoleService
    {
        private const string PermissionClaimType = "permission";

        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IRoleClaimRepository _roleClaimRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleService(
            RoleManager<ApplicationRole> roleManager,
            IRoleClaimRepository roleClaimRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _roleClaimRepository = roleClaimRepository ?? throw new ArgumentNullException(nameof(roleClaimRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private int GetCurrentUserIdOrDefault() {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return int.TryParse(userIdStr, out var id) ? id : CommonConstant.DEFAULT_SYSTEM_USER;
        }
        public Task<IReadOnlyList<ApplicationRole>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            // Rely on global query filters for tenant scoping
            var roles = _roleManager.Roles
                .OrderBy(r => r.Name)
                .ToList();
            return Task.FromResult<IReadOnlyList<ApplicationRole>>(roles);
        }

        public async Task<ApplicationRole?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return null;
            
            // Tenant scope is enforced by global query filters
            return role;
        }

        public async Task<ApplicationRole> CreateRoleAsync(CreateRoleRequest request, int? createdBy = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Role name is required.");

            // Check if role with same name exists (tenant filtered implicitly)
            var existingRole = _roleManager.Roles
                .Where(r => r.NormalizedName == request.Name.ToUpperInvariant())
                .FirstOrDefault();
                
            if (existingRole != null) throw new InvalidOperationException($"Role '{request.Name}' already exists in this tenant.");

            var now = DateTime.UtcNow;
            var userId = createdBy ?? GetCurrentUserIdOrDefault();

            var role = new ApplicationRole
            {
                Name = request.Name,
                NormalizedName = request.Name.ToUpperInvariant(),
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role: {errors}");
            }
            return role;
        }

        public async Task<ApplicationRole> UpdateRoleAsync(int id, UpdateRoleRequest request, int? updatedBy = null, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) throw new InvalidOperationException($"Role with ID '{id}' not found.");
            
            var oldName = role.Name ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(request.Name) && !string.Equals(oldName, request.Name, StringComparison.Ordinal))
            {
                // Rely on global filters for tenant scoping
                var dup = _roleManager.Roles
                    .Where(r => r.NormalizedName == request.Name.ToUpperInvariant())
                    .FirstOrDefault();
                    
                if (dup != null && dup.Id != role.Id)
                    throw new InvalidOperationException($"Role '{request.Name}' already exists in this tenant.");

                role.Name = request.Name;
                role.NormalizedName = request.Name.ToUpperInvariant();
            }

            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = updatedBy ?? GetCurrentUserIdOrDefault();

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update role: {errors}");
            }
            return role;
        }

        public async Task<bool> DeleteRoleAsync(int id, int? deletedBy = null, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return false;
            
            // Prevent deletion of built-in roles (optional safeguard)
            if (CommonConstant.RoleNames.All.Contains(role.Name ?? string.Empty))
                throw new InvalidOperationException($"Built-in role '{role.Name}' cannot be deleted.");

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to delete role: {errors}");
            }
            return true;
        }

        public async Task<string[]> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new InvalidOperationException($"Role with ID '{roleId}' not found.");
            
            // Tenant scope is enforced by global query filters
            var claims = await _roleManager.GetClaimsAsync(role);
            return claims.Where(c => c.Type == PermissionClaimType).Select(c => c.Value).Distinct().OrderBy(v => v).ToArray();
        }

        public async Task<string[]> SetRolePermissionsAsync(int roleId, IEnumerable<string> permissionNames, int? updatedBy = null, CancellationToken cancellationToken = default)
        {
            if (permissionNames == null) permissionNames = Array.Empty<string>();
            var desired = new HashSet<string>(permissionNames.Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)));

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new InvalidOperationException($"Role with ID '{roleId}' not found.");
            
            // Tenant scope is enforced by global query filters

            // Load existing claims
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var current = new HashSet<string>(existingClaims.Where(c => c.Type == PermissionClaimType).Select(c => c.Value));

            var toAdd = desired.Except(current).ToArray();
            var toRemove = current.Except(desired).ToArray();

            var userId = updatedBy ?? GetCurrentUserIdOrDefault();
            var now = DateTime.UtcNow;

            // Remove claims no longer desired
            if (toRemove.Length > 0)
            {
                await _roleClaimRepository.RemoveRoleClaimsAsync(role.Id, PermissionClaimType, toRemove, cancellationToken);
            }

            // Add new claims with audit fields
            if (toAdd.Length > 0)
            {
                var claimsToAdd = toAdd.Select(perm => new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = PermissionClaimType,
                    ClaimValue = perm,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                await _roleClaimRepository.AddRoleClaimsAsync(claimsToAdd, cancellationToken);
            }
            return await GetRolePermissionsAsync(roleId, cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationRoleClaim>> GetRolePermissionClaimsAsync(int roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString()) ?? throw new InvalidOperationException($"Role with ID '{roleId}' not found.");
            
            // Tenant scope is enforced by global query filters
            var claims = await _roleClaimRepository.GetRoleClaimsAsync(role.Id, PermissionClaimType, cancellationToken);
            return claims;
        }
    }
}
