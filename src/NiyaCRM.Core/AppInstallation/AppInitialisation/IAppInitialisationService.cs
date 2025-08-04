using System.Threading.Tasks;

namespace NiyaCRM.Core.AppInstallation.AppInitialisation;

public interface IAppInitialisationService
{
    Task InitialiseAppAsync(CancellationToken cancellationToken = default);
}

