using System;
using System.Threading;
using System.Threading.Tasks;
using OXDesk.Core.Tenants;
using OXDesk.Core.AuditLogs;

namespace OXDesk.Core
{
    /// <summary>
    /// Defines the Unit of Work contract for coordinating changes across repositories.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets a repository instance for the specified type.
        /// </summary>
        /// <typeparam name="TRepository">The repository type to retrieve.</typeparam>
        /// <returns>An instance of the specified repository type.</returns>
        TRepository GetRepository<TRepository>() where TRepository : class;
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Begins a transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Commits the current transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Rolls back the current transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
