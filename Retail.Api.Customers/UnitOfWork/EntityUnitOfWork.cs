using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Retail.Api.Customers.Data;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Repositories;

namespace Retail.Api.Customers.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class EntityUnitOfWork : IEntityUnitOfWork
    {
        private readonly ApplicationDbContext _entityContext;
        private IDbContextTransaction? _entityTransaction;
        private ICustomerEntityRepository? _customerEntityRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUnitOfWork"/> class.
        /// </summary>
        /// <param name="entityContext">Entity framework Db context.</param>
        public EntityUnitOfWork(ApplicationDbContext entityContext)
        {
            _entityContext = entityContext;
        }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        public ICustomerEntityRepository CustomerEntityRepository
        {
            get
            {
                if (_customerEntityRepository == null)
                {
                    _customerEntityRepository = new CustomerEntityRepository(_entityContext);
                }

                return _customerEntityRepository;
            }
        }

        /// <summary>
        /// Method to begin transaction.
        /// </summary>
        public void BeginTransaction()
        {
            _entityTransaction = _entityContext.Database.BeginTransaction();
        }

        /// <summary>
        /// Method to begin asynchronously.
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _entityTransaction = await _entityContext.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Method to commit changes.
        /// </summary>
        public void Commit()
        {
            _entityContext.SaveChanges();
            _entityTransaction?.Commit();
        }

        /// <summary>
        /// Method to commit changes asynchronously.
        /// </summary>
        public async Task CommitAsync()
        {
            await _entityContext.SaveChangesAsync();

            if (_entityTransaction is not null)
            {
                await _entityTransaction.CommitAsync();
            }
        }

        /// <summary>
        /// Method to rollback changes.
        /// </summary>
        public void Rollback()
        {
            _entityTransaction?.Rollback();
            _entityTransaction?.Dispose();
            _entityContext.Dispose();
        }

        /// <summary>
        /// Method to rollback changes asynchronously.
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_entityTransaction is not null)
            {
                await _entityTransaction.RollbackAsync();
                await _entityTransaction.DisposeAsync();
            }

            await _entityContext.DisposeAsync();
        }
    }
}
