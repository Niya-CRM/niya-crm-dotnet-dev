using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.DynamicObjects.DTOs;

namespace OXDesk.Core.DynamicObjects
{
    /// <summary>
    /// Factory interface for building DynamicObject DTOs and response wrappers.
    /// </summary>
    public interface IDynamicObjectFactory
    {
        Task<PagedListWithRelatedResponse<DynamicObjectResponse>> BuildListAsync(IEnumerable<DynamicObject> items, CancellationToken cancellationToken = default);

        Task<EntityWithRelatedResponse<DynamicObjectResponse, DynamicObjectDetailsRelated>> BuildDetailsAsync(DynamicObject item, CancellationToken cancellationToken = default);
    }
}
