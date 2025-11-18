using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using OXDesk.Core;
using OXDesk.Core.Tenants;
using OXDesk.Core.AuditLogs;

namespace OXDesk.Infrastructure.Data
{
    /// <summary>
    /// Implements the Unit of Work pattern for coordinating repository changes.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TenantDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, object> _repositories = new();
        private IDbContextTransaction? _transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="dbContext">The tenant database context.</param>
        /// <param name="serviceProvider">The service provider for repository resolution.</param>
        public UnitOfWork(
            TenantDbContext dbContext,
            IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets a repository instance for the specified type.
        /// </summary>
        /// <typeparam name="TRepository">The repository type to retrieve.</typeparam>
        /// <returns>An instance of the specified repository type.</returns>
        public TRepository GetRepository<TRepository>() where TRepository : class
        {
            var type = typeof(TRepository);
            if (!_repositories.TryGetValue(type, out var repository))
            {
                repository = _serviceProvider.GetRequiredService<TRepository>();
                _repositories[type] = repository;
            }
            return (TRepository)repository;
        }
        
        /// <summary>
        /// Saves all changes made in this context to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private bool _disposed = false;

        /// <summary>
        /// Releases the unmanaged resources used by the UnitOfWork and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _transaction = null;
                    _dbContext.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer for the UnitOfWork class.
        /// </summary>
        ~UnitOfWork()
        {
            Dispose(false);
        }
        
        /// <summary>
        /// Begins a transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }
            
            _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }
        
        /// <summary>
        /// Commits the current transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }
            
            try
            {
                await _transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        
        /// <summary>
        /// Rolls back the current transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to roll back.");
            }
            
            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}
