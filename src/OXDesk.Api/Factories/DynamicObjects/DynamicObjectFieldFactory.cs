using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.DynamicObjects.Fields;
using OXDesk.Core.DynamicObjects.Fields.DTOs;
using OXDesk.Core.Identity;

namespace OXDesk.Api.Factories.DynamicObjects
{
    /// <summary>
    /// Builds DynamicObjectField response DTOs and wraps them with related data.
    /// </summary>
    public sealed class DynamicObjectFieldFactory : IDynamicObjectFieldFactory
    {
        private readonly IUserService _userService;

        public DynamicObjectFieldFactory(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private static DynamicObjectFieldResponse Map(DynamicObjectField entity) => new DynamicObjectFieldResponse
        {
            Id = entity.Id,
            ObjectId = entity.ObjectId,
            Label = entity.Label,
            FieldTypeId = entity.FieldTypeId,
            Indexed = entity.Indexed,
            Description = entity.Description,
            HelpText = entity.HelpText,
            Placeholder = entity.Placeholder,
            Required = entity.Required,
            Unique = entity.Unique,
            MinLength = entity.MinLength,
            MaxLength = entity.MaxLength,
            Decimals = entity.Decimals,
            MaxFileSize = entity.MaxFileSize,
            AllowedFileTypes = entity.AllowedFileTypes,
            MinFileCount = entity.MinFileCount,
            MaxFileCount = entity.MaxFileCount,
            ValueListId = entity.ValueListId,
            MinSelectedItems = entity.MinSelectedItems,
            MaxSelectedItems = entity.MaxSelectedItems,
            Editable = entity.Editable,
            VisibleOnCreate = entity.VisibleOnCreate,
            VisibleOnEdit = entity.VisibleOnEdit,
            VisibleOnView = entity.VisibleOnView,
            AuditChanges = entity.AuditChanges,
            CreatedAt = entity.CreatedAt,
            DeletedAt = entity.DeletedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };

        private async Task EnrichAsync(DynamicObjectFieldResponse dto, CancellationToken cancellationToken)
        {
            dto.CreatedByText = await _userService.GetUserNameByIdAsync(dto.CreatedBy, cancellationToken);
            dto.UpdatedByText = await _userService.GetUserNameByIdAsync(dto.UpdatedBy, cancellationToken);
        }

        public async Task<PagedListWithRelatedResponse<DynamicObjectFieldResponse>> BuildListAsync(IEnumerable<DynamicObjectField> fields, CancellationToken cancellationToken = default)
        {
            var list = fields.Select(Map).ToList();
            foreach (var item in list)
            {
                await EnrichAsync(item, cancellationToken);
            }
            return new PagedListWithRelatedResponse<DynamicObjectFieldResponse>
            {
                Data = list,
                PageNumber = 1,
                RowCount = list.Count,
                Related = Array.Empty<object>()
            };
        }

        public async Task<EntityWithRelatedResponse<DynamicObjectFieldResponse, DynamicObjectFieldDetailsRelated>> BuildDetailsAsync(DynamicObjectField field, CancellationToken cancellationToken = default)
        {
            var dto = Map(field);
            await EnrichAsync(dto, cancellationToken);
            return new EntityWithRelatedResponse<DynamicObjectFieldResponse, DynamicObjectFieldDetailsRelated>
            {
                Data = dto,
                Related = new DynamicObjectFieldDetailsRelated()
            };
        }
    }
}
