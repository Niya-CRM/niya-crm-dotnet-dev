using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Cache;
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
        private readonly ILogger<RoleService> _logger;
        private readonly IAuditLogService _auditLogService;
        private readonly IChangeHistoryLogService _changeHistoryLogService;
        private readonly ICacheService _cacheService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleService(
            RoleManager<ApplicationRole> roleManager,
            IRoleClaimRepository roleClaimRepository,
            ILogger<RoleService> logger,
            IAuditLogService auditLogService,
            IChangeHistoryLogService changeHistoryLogService,
            ICacheService cacheService,
            IHttpContextAccessor httpContextAccessor)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _roleClaimRepository = roleClaimRepository ?? throw new ArgumentNullException(nameof(roleClaimRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _changeHistoryLogService = changeHistoryLogService ?? throw new ArgumentNullException(nameof(changeHistoryLogService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private Guid GetCurrentUserIdOrDefault() {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdStr, out var id) ? id : CommonConstant.DEFAULT_SYSTEM_USER;
        }
        private string GetUserIp() => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

        private static RoleResponse MapToRoleResponse(ApplicationRole role) => new RoleResponse
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            NormalizedName = role.NormalizedName ?? string.Empty,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            CreatedBy = role.CreatedBy,
            UpdatedBy = role.UpdatedBy
        };

        public async Task<IReadOnlyList<RoleResponse>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            return await Task.FromResult(roles.Select(MapToRoleResponse).ToList());
        }

        public async Task<RoleResponse?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            return role == null ? null : MapToRoleResponse(role);
        }

        public async Task<RoleDetailsResponse?> GetRoleDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return null;
            var claims = await _roleManager.GetClaimsAsync(role);
            var permValues = claims.Where(c => c.Type == PermissionClaimType).Select(c => c.Value).Distinct().OrderBy(v => v).ToArray();
            return new RoleDetailsResponse
            {
                Data = MapToRoleResponse(role),
                Permissions = permValues
            };
        }

        public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, Guid? createdBy = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Role name is required.");

            var existing = await _roleManager.FindByNameAsync(request.Name);
            if (existing != null) throw new InvalidOperationException($"Role '{request.Name}' already exists.");

            var now = DateTime.UtcNow;
            var userId = createdBy ?? GetCurrentUserIdOrDefault();

            var role = new ApplicationRole
            {
                Id = Guid.CreateVersion7(),
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

            await _auditLogService.CreateAuditLogAsync(
                objectKey: CommonConstant.MODULE_ROLE,
                @event: CommonConstant.AUDIT_LOG_EVENT_CREATE,
                objectItemId: role.Id.ToString(),
                data: $"Role created: {role.Name}",
                ip: GetUserIp(),
                createdBy: userId,
                cancellationToken: cancellationToken);

            await _cacheService.RemoveAsync(CommonConstant.CacheKeys.RolesList);
            return MapToRoleResponse(role);
        }

        public async Task<RoleResponse> UpdateRoleAsync(Guid id, UpdateRoleRequest request, Guid? updatedBy = null, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) throw new InvalidOperationException($"Role with ID '{id}' not found.");

            var oldName = role.Name ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(request.Name) && !string.Equals(oldName, request.Name, StringComparison.Ordinal))
            {
                var dup = await _roleManager.FindByNameAsync(request.Name);
                if (dup != null && dup.Id != role.Id)
                    throw new InvalidOperationException($"Role '{request.Name}' already exists.");

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

            // Change history
            if (!string.Equals(oldName, role.Name, StringComparison.Ordinal))
            {
                await _changeHistoryLogService.CreateChangeHistoryLogAsync(
                    objectKey: CommonConstant.MODULE_ROLE,
                    objectItemId: role.Id,
                    fieldName: "name",
                    oldValue: oldName,
                    newValue: role.Name,
                    createdBy: role.UpdatedBy,
                    cancellationToken: cancellationToken);
            }

            await _auditLogService.CreateAuditLogAsync(
                objectKey: CommonConstant.MODULE_ROLE,
                @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
                objectItemId: role.Id.ToString(),
                data: $"Role updated: {role.Name}",
                ip: GetUserIp(),
                createdBy: role.UpdatedBy,
                cancellationToken: cancellationToken);

            await _cacheService.RemoveAsync(CommonConstant.CacheKeys.RolesList);
            return MapToRoleResponse(role);
        }

        public async Task<bool> DeleteRoleAsync(Guid id, Guid? deletedBy = null, CancellationToken cancellationToken = default)
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

            var userId = deletedBy ?? GetCurrentUserIdOrDefault();
            await _changeHistoryLogService.CreateChangeHistoryLogAsync(
                objectKey: CommonConstant.MODULE_ROLE,
                objectItemId: role.Id,
                fieldName: CommonConstant.ChangeHistoryFields.Deleted,
                oldValue: "N",
                newValue: "Y",
                createdBy: userId,
                cancellationToken: cancellationToken);

            await _auditLogService.CreateAuditLogAsync(
                objectKey: CommonConstant.MODULE_ROLE,
                @event: CommonConstant.AUDIT_LOG_EVENT_DELETE,
                objectItemId: id.ToString(),
                data: $"Role deleted: {role.Name}",
                ip: GetUserIp(),
                createdBy: userId,
                cancellationToken: cancellationToken);

            await _cacheService.RemoveAsync(CommonConstant.CacheKeys.RolesList);
            return true;
        }

        public async Task<string[]> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new InvalidOperationException($"Role with ID '{roleId}' not found.");
            var claims = await _roleManager.GetClaimsAsync(role);
            return claims.Where(c => c.Type == PermissionClaimType).Select(c => c.Value).Distinct().OrderBy(v => v).ToArray();
        }

        public async Task<string[]> SetRolePermissionsAsync(Guid roleId, IEnumerable<string> permissionNames, Guid? updatedBy = null, CancellationToken cancellationToken = default)
        {
            if (permissionNames == null) permissionNames = Array.Empty<string>();
            var desired = new HashSet<string>(permissionNames.Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)));

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new InvalidOperationException($"Role with ID '{roleId}' not found.");

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

            await _auditLogService.CreateAuditLogAsync(
                objectKey: CommonConstant.MODULE_ROLE,
                @event: CommonConstant.AUDIT_LOG_EVENT_UPDATE,
                objectItemId: role.Id.ToString(),
                data: $"Role permissions updated: +[{string.Join(",", toAdd)}], -[{string.Join(",", toRemove)}]",
                ip: GetUserIp(),
                createdBy: userId,
                cancellationToken: cancellationToken);

            return await GetRolePermissionsAsync(roleId, cancellationToken);
        }
    }
}
