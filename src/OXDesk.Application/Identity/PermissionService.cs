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

namespace OXDesk.Application.Identity;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const string PermissionClaimType = "permission";

    public PermissionService(
        IPermissionRepository permissionRepository,
        RoleManager<ApplicationRole> roleManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    private Guid GetCurrentUserIdOrDefault()
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
    }
    public async Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var entities = (await _permissionRepository.GetAllAsync())
            .OrderBy(e => e.Name)
            .ToList();
        return entities;
    }

    public async Task<Permission?> GetPermissionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _permissionRepository.GetByIdAsync(id);
        return entity;
    }

    public async Task<Permission> CreatePermissionAsync(CreatePermissionRequest request, Guid? createdBy = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Permission name is required.");
        var name = request.Name.Trim();
        var normalized = name.ToUpperInvariant();
        
        var existing = await _permissionRepository.GetByNameAsync(normalized);
            
        if (existing != null) throw new InvalidOperationException($"A permission with the same name already exists in this tenant.");

        var now = DateTime.UtcNow;
        var userId = createdBy ?? GetCurrentUserIdOrDefault();
        if (userId == Guid.Empty)
            throw new InvalidOperationException("Unable to determine current user for audit.");

        var entity = new Permission
        {
            Name = name,
            NormalizedName = normalized,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        entity = await _permissionRepository.AddAsync(entity);
        return entity;
    }

    public async Task<Permission> UpdatePermissionAsync(int id, UpdatePermissionRequest request, Guid? updatedBy = null, CancellationToken cancellationToken = default)
    {
        var entity = await _permissionRepository.GetByIdAsync(id);
        if (entity == null) throw new InvalidOperationException($"Permission with ID '{id}' was not found.");

        var newName = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(newName)) throw new InvalidOperationException("Permission name is required.");
        var newNormalized = newName.ToUpperInvariant();

        // uniqueness check within tenant
        var dup = await _permissionRepository.GetByNameAsync(newNormalized);
            
        if (dup != null && dup.Id != entity.Id)
        {
            throw new InvalidOperationException("A permission with the same name already exists in this tenant.");
        }

        entity.Name = newName;
        entity.NormalizedName = newNormalized;
        entity.UpdatedAt = DateTime.UtcNow;
        var userId = updatedBy ?? GetCurrentUserIdOrDefault();
        if (userId == Guid.Empty)
            throw new InvalidOperationException("Unable to determine current user for audit.");
        entity.UpdatedBy = userId;

        entity = await _permissionRepository.UpdateAsync(entity);
        return entity;
    }

    // Support methods used by validators and potential maintenance ops
    public async Task<Permission?> GetPermissionByNameAsync(string normalizedName)
    {
        var permission = await _permissionRepository.GetByNameAsync(normalizedName);
        return permission;
    }

    public async Task<bool> DeletePermissionAsync(int id)
    {
        return await _permissionRepository.DeleteAsync(id);
    }

    public async Task<string[]> GetPermissionRolesAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission == null) throw new InvalidOperationException($"Permission with ID '{permissionId}' was not found in the current tenant.");


        // Roles are filtered by global query filters
        var roles = _roleManager.Roles.ToList();
        var rolesWithPermission = new List<string>();
        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            if (claims.Any(c => c.Type == PermissionClaimType && string.Equals(c.Value, permission.Name, StringComparison.Ordinal)))
            {
                if (!string.IsNullOrWhiteSpace(role.Name))
                {
                    rolesWithPermission.Add(role.Name);
                }
            }
        }
        return rolesWithPermission.Distinct().OrderBy(r => r).ToArray();
    }
}

