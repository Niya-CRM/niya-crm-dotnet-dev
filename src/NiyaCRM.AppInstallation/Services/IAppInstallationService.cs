using System.Threading.Tasks;

namespace NiyaCRM.AppInstallation.Services
{
    /// <summary>
    /// Interface for application installation and upgrade operations
    /// </summary>
    public interface IAppInstallationService
    {
        /// <summary>
        /// Performs initial application setup including data seeding
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task InitializeApplicationAsync();
        
        /// <summary>
        /// Checks current application version and performs any necessary upgrades
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpgradeApplicationAsync();
        
        /// <summary>
        /// Gets the current application version from the database
        /// </summary>
        /// <returns>The current application version</returns>
        Task<string> GetCurrentVersionAsync();
    }
}
