using System.Threading.Tasks;

namespace OXDesk.Core.AppInstallation.AppInitialisation;

public interface IAppInitialisationService
{
    Task InitialiseAppAsync(CancellationToken cancellationToken = default);
}

