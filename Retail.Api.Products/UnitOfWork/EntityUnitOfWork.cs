using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Retail.Api.Products.Data;
using Retail.Api.Products.DefaultInterface;
using Retail.Api.Products.Interface;
using Retail.Api.Products.Model;
using Retail.Api.Products.Repositories;

namespace Retail.Api.Products.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class EntityUnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _entityContext;
        private IDbContextTransaction? _entityTransaction;
        private IRepository<Sku>? _productRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUnitOfWork"/> class.
        /// </summary>
        /// <param name="entityContext">Entity framework Db context.</param>
        public EntityUnitOfWork(ApplicationDbContext entityContext)
        {
            _entityContext = entityContext;
        }

        /// <summary>
        /// Gets or sets product repository.
        /// </summary>
        public IRepository<Sku> ProductRepository
        {
            get
            {
                if (_productRepository == null)
                {
                    _productRepository = new ProductEntityRepository(_entityContext);
                }

                return _productRepository;
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
        /// Method to commit changes.
        /// </summary>
        public void Commit()
        {
            _entityContext.SaveChanges();
            _entityTransaction?.Commit();
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
    }
}
