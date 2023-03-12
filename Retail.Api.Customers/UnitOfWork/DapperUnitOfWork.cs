using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Retail.Api.Customers.Data;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Model;
using Retail.Api.Customers.Repositories;
using System.Data;

namespace Retail.Api.Customers.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class DapperUnitOfWork : IDapperUnitOfWork
    {
        private readonly DapperContext _dapperContext;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private ICustomerDapperRepository? _customerDapperRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="dapperContext">Dapper Db context.</param>
        public DapperUnitOfWork(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
            _connection = _connection ?? _dapperContext.CreateConnection();
        }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        public ICustomerDapperRepository CustomerDapperRepository
        {
            get
            {
                if (_customerDapperRepository == null)
                {
                    _customerDapperRepository = new CustomerDapperRepository(_dapperContext);
                }

                return _customerDapperRepository;
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