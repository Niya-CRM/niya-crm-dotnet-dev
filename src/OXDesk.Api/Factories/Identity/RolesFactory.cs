using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using OXDesk.Core.ValueLists;
using OXDesk.Core.ValueLists.DTOs;

namespace OXDesk.Api.Factories.Identity
{
    /// <summary>
    /// Builds Role response DTOs and wraps them with related data.
    /// </summary>
    public sealed class RolesFactory : IRoleFactory
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IValueListService _valueListService;

        public RolesFactory(IUserService userService, IRoleService roleService, IValueListService valueListService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _valueListService = valueListService ?? throw new ArgumentNullException(nameof(valueListService));
        }

        private static RoleResponse Map(ApplicationRole role) => new RoleResponse
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            NormalizedName = role.NormalizedName ?? string.Empty,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            CreatedBy = role.CreatedBy,
            UpdatedBy = role.UpdatedBy
        };

        private async Task EnrichAsync(RoleResponse dto, CancellationToken cancellationToken)
        {
            dto.CreatedByText = await _userService.GetUserNameByIdAsync(dto.CreatedBy, cancellationToken);
            dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);
        }

        public async Task<PagedListWithRelatedResponse<RoleResponse>> BuildListAsync(IEnumerable<ApplicationRole> roles, CancellationToken cancellationToken = default)
        {
            var list = roles.Select(Map).ToList();
            foreach (var item in list)
            {
                await EnrichAsync(item, cancellationToken);
            }

            return new PagedListWithRelatedResponse<RoleResponse>
            {
                Data = list,
                PageNumber = 1,
                RowCount = list.Count,
                Related = Array.Empty<object>()
            };
        }

        public async Task<EntityWithRelatedResponse<RoleResponse, RoleDetailsRelated>> BuildDetailsAsync(ApplicationRole role, CancellationToken cancellationToken = default)
        {
            var dto = Map(role);
            await EnrichAsync(dto, cancellationToken);

            // Fetch related data (permissions) directly in the factory
            var permissions = await _roleService.GetRolePermissionsAsync(role.Id, cancellationToken);

            return new EntityWithRelatedResponse<RoleResponse, RoleDetailsRelated>
            {
                Data = dto,
                Related = new RoleDetailsRelated { Permissions = permissions }
            };
        }

        public async Task<PagedListWithRelatedResponse<RolePermissionResponse>> BuildPermissionsListAsync(IEnumerable<ApplicationRoleClaim> claims, CancellationToken cancellationToken = default)
        {
            var list = new List<RolePermissionResponse>();
            foreach (var c in claims)
            {
                var item = new RolePermissionResponse
                {
                    Name = c.ClaimValue ?? string.Empty,
                    CreatedBy = c.CreatedBy,
                    CreatedAt = c.CreatedAt,
                    UpdatedBy = c.UpdatedBy,
                    UpdatedAt = c.UpdatedAt,
                };
                item.CreatedByText = await _userService.GetUserNameByIdAsync(item.CreatedBy, cancellationToken);
                item.UpdatedByText = await _userService.GetUserNameByIdAsync(item.UpdatedBy, cancellationToken);
                list.Add(item);
            }            

            return new PagedListWithRelatedResponse<RolePermissionResponse>
            {
                Data = list,
                PageNumber = 1,
                RowCount = list.Count,
                Related = Array.Empty<object>()
            };
        }
    }
}
