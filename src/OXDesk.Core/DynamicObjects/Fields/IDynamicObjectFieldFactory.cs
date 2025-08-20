using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.DynamicObjects.Fields.DTOs;

namespace OXDesk.Core.DynamicObjects.Fields
{
    /// <summary>
    /// Factory interface for building DynamicObjectField response DTOs and wrappers.
    /// </summary>
    public interface IDynamicObjectFieldFactory
    {
        Task<PagedListWithRelatedResponse<DynamicObjectFieldResponse>> BuildListAsync(IEnumerable<DynamicObjectField> fields, CancellationToken cancellationToken = default);

        Task<EntityWithRelatedResponse<DynamicObjectFieldResponse, DynamicObjectFieldDetailsRelated>> BuildDetailsAsync(DynamicObjectField field, CancellationToken cancellationToken = default);
    }
}
