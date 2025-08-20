using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Api.Factories
{
    /// <summary>
    /// Builds Permission response DTOs and wraps them with related data.
    /// </summary>
    public sealed class PermissionsFactory : IPermissionFactory
    {
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;

        public PermissionsFactory(IUserService userService, IPermissionService permissionService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        private static PermissionResponse Map(Permission permission) => new PermissionResponse
        {
            Id = permission.Id,
            Name = permission.Name ?? string.Empty,
            NormalizedName = permission.NormalizedName ?? string.Empty,
            CreatedAt = permission.CreatedAt,
            UpdatedAt = permission.UpdatedAt,
            CreatedBy = permission.CreatedBy,
            UpdatedBy = permission.UpdatedBy
        };

        private async Task EnrichAsync(PermissionResponse dto, CancellationToken cancellationToken)
        {
            dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);
        }

        public async Task<PagedListWithRelatedResponse<PermissionResponse>> BuildListAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken = default)
        {
            var list = permissions.Select(Map).ToList();
            foreach (var item in list)
            {
                await EnrichAsync(item, cancellationToken);
            }

            return new PagedListWithRelatedResponse<PermissionResponse>
            {
                Data = list,
                PageNumber = 1,
                RowCount = list.Count,
                Related = Array.Empty<object>()
            };
        }

        public async Task<EntityWithRelatedResponse<PermissionResponse, PermissionDetailsRelated>> BuildDetailsAsync(Permission permission, CancellationToken cancellationToken = default)
        {
            var dto = Map(permission);
            await EnrichAsync(dto, cancellationToken);

            var roles = await _permissionService.GetPermissionRolesAsync(permission.Id, cancellationToken);

            return new EntityWithRelatedResponse<PermissionResponse, PermissionDetailsRelated>
            {
                Data = dto,
                Related = new PermissionDetailsRelated { Roles = roles }
            };
        }
    }
}
