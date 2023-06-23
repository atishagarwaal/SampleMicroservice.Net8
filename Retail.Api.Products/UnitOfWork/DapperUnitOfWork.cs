using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Retail.Api.Products.Data;
using Retail.Api.Products.DefaultInterface;
using Retail.Api.Products.DefaultRepositories;
using Retail.Api.Products.Interface;
using Retail.Api.Products.Model;
using Retail.Api.Products.Repositories;
using System.Data;

namespace Retail.Api.Products.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class DapperUnitOfWork : IUnitOfWork
    {
        private readonly DapperContext _dapperContext;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private IRepository<Sku>? _productRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="dapperContext">Dapper Db context.</param>
        public DapperUnitOfWork(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        /// <summary>
        /// Gets or sets connection.
        /// </summary>
        private IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = _dapperContext.CreateConnection();
                }

                return _connection;
            }
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
                    _productRepository = new ProductDapperRepository(_dapperContext);
                }

                return _productRepository;
            }
        }

        /// <summary>
        /// Method to begin transaction.
        /// </summary>
        public void BeginTransaction()
        {
            _transaction = _connection?.BeginTransaction();
        }

        /// <summary>
        /// Method to commit changes.
        /// </summary>
        public void Commit()
        {
            _transaction?.Commit();
        }

        /// <summary>
        /// Method to rollback changes.
        /// </summary>
        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }
}