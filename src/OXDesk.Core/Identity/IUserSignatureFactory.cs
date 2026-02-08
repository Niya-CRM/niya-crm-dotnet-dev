using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Identity.DTOs;

namespace OXDesk.Core.Identity;

/// <summary>
/// Factory interface for building user signature response DTOs.
/// </summary>
public interface IUserSignatureFactory
{
    /// <summary>
    /// Builds a response DTO from a user signature entity.
    /// </summary>
    /// <param name="userSignature">The user signature entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user signature response DTO.</returns>
    Task<UserSignatureResponse> BuildResponseAsync(UserSignature userSignature, CancellationToken cancellationToken = default);
}
