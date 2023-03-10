using Microsoft.EntityFrameworkCore;
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
        /// Method to commit changes.
        /// </summary>
        public void Commit()
        {
            _entityContext.SaveChanges();
        }

        /// <summary>
        /// Method to commit changes asynchronously.
        /// </summary>
        public async Task CommitAsync()
        { 
            await _entityContext.SaveChangesAsync();
        }

        /// <summary>
        /// Method to rollback changes.
        /// </summary>
        public void Rollback()
        {
            _entityContext.Dispose();
        }

        /// <summary>
        /// Method to rollback changes asynchronously.
        /// </summary>
        public async Task RollbackAsync()
        { 
            await _entityContext.DisposeAsync();
        }
    }
}
