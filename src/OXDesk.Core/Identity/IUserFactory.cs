using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Common.DTOs;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Core.Identity
{
    /// <summary>
    /// Factory interface for building user DTOs and response wrappers with enrichment.
    /// Placed in Core so controllers depend on abstractions.
    /// </summary>
    public interface IUserFactory
    {
        /// <summary>
        /// Builds a paged list response from ApplicationUser entities with display enrichments.
        /// Related is empty to match list patterns in the API.
        /// </summary>
        Task<PagedListWithRelatedResponse<UserResponse>> BuildListAsync(IEnumerable<ApplicationUser> users, CancellationToken cancellationToken = default);

        /// <summary>
        /// Builds a single enriched user response (display texts populated).
        /// </summary>
        Task<UserResponse> BuildItemAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Builds a user details response with related reference data (countries, profiles, time zones, statuses).
        /// </summary>
        Task<EntityWithRelatedResponse<UserResponse, UserDetailsRelated>> BuildDetailsAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    }
}
