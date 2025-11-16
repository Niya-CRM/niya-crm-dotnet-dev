using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.DynamicObjects.DTOs;
using OXDesk.Core.Identity;

namespace OXDesk.Api.Factories.DynamicObjects
{
    /// <summary>
    /// Builds DynamicObject response DTOs and wraps them with related data.
    /// </summary>
    public sealed class DynamicObjectFactory : IDynamicObjectFactory
    {
        private readonly IUserService _userService;

        public DynamicObjectFactory(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private static DynamicObjectResponse Map(DynamicObject entity) => new DynamicObjectResponse
        {
            Id = entity.Id,
            ObjectName = entity.ObjectName,
            SingularName = entity.SingularName,
            PluralName = entity.PluralName,
            ObjectKey = entity.ObjectKey,
            Description = entity.Description,
            ObjectType = entity.ObjectType,
            CreatedAt = entity.CreatedAt,
            DeletedAt = entity.DeletedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };

        private async Task EnrichAsync(DynamicObjectResponse dto, CancellationToken cancellationToken)
        {
            dto.CreatedByText = await _userService.GetUserNameByIdAsync(dto.CreatedBy, cancellationToken);
            dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);
        }

        public async Task<PagedListWithRelatedResponse<DynamicObjectResponse>> BuildListAsync(IEnumerable<DynamicObject> items, CancellationToken cancellationToken = default)
        {
            var list = items.Select(Map).ToList();
            foreach (var item in list)
            {
                await EnrichAsync(item, cancellationToken);
            }
            return new PagedListWithRelatedResponse<DynamicObjectResponse>
            {
                Data = list,
                PageNumber = 1,
                RowCount = list.Count,
                Related = Array.Empty<object>()
            };
        }

        public async Task<EntityWithRelatedResponse<DynamicObjectResponse, DynamicObjectDetailsRelated>> BuildDetailsAsync(DynamicObject item, CancellationToken cancellationToken = default)
        {
            var dto = Map(item);
            await EnrichAsync(dto, cancellationToken);
            return new EntityWithRelatedResponse<DynamicObjectResponse, DynamicObjectDetailsRelated>
            {
                Data = dto,
                Related = new DynamicObjectDetailsRelated()
            };
        }
    }
}
