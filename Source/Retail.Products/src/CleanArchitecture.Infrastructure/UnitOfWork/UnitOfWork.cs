using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories;

namespace Retail.Api.Products.src.CleanArchitecture.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction _transaction;
        public ISkuRepository Skus { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="entityContext">Entity framework Db context.</param>
        /// <param name="logger">Instance of logger.</param>
        public UnitOfWork(ApplicationDbContext entityContext, ILogger<UnitOfWork> logger)
        {
            _context = entityContext;
            _logger = logger;
            Skus = new SkuRepository(_context);
        }

        /// <summary>
        /// Method to begin transaction.
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _logger.LogDebug("Beginning database transaction");
                _transaction = await _context.Database.BeginTransactionAsync();
                _logger.LogDebug("Database transaction started");
            }
        }

        /// <summary>
        /// Method to commit changes.
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                _logger.LogDebug("Committing database transaction");
                await _transaction.CommitAsync();
                _transaction.Dispose();
                _transaction = null;
                _logger.LogDebug("Database transaction committed");
            }
        }

        /// <summary>
        /// Method to rollback changes.
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                _logger.LogWarning("Rolling back database transaction");
                await _transaction.RollbackAsync();
                _transaction.Dispose();
                _transaction = null;
                _logger.LogWarning("Database transaction rolled back");
            }
        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public async Task<int> CompleteAsync()
        {
            _logger.LogDebug("Saving changes to database");
            var result = await _context.SaveChangesAsync();
            _logger.LogDebug("Saved {ChangeCount} changes to database", result);
            return result;
        }

        /// <summary>
        /// Disposes the unit of work and any active transaction.
        /// </summary>
        public void Dispose()
        {
            if (_transaction != null)
            {
                _logger.LogDebug("Disposing database transaction");
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }
}
