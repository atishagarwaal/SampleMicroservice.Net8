using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Retail.Api.Orders.Data;
using Retail.Api.Orders.DefaultInterface;
using Retail.Api.Orders.Interface;
using Retail.Api.Orders.Model;
using Retail.Api.Orders.Repositories;
using System.Data;

namespace Retail.Api.Orders.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class DapperUnitOfWork : IDapperUnitOfWork
    {
        private readonly DapperContext _dapperContext;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private IOrderDapperRepository? _orderDapperRepository;
        private ILineItemDapperRepository? _lineItemDapperRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="dapperContext">Dapper Db context.</param>
        public DapperUnitOfWork(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        public IOrderDapperRepository OrderDapperRepository
        {
            get
            {
                if (_orderDapperRepository == null)
                {
                    _orderDapperRepository = new OrderDapperRepository(_dapperContext);
                }

                return _orderDapperRepository;
            }
        }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        public ILineItemDapperRepository LineItemDapperRepository
        {
            get
            {
                if (_lineItemDapperRepository == null)
                {
                    _lineItemDapperRepository = new LineItemDapperRepository(_dapperContext);
                }

                return _lineItemDapperRepository;
            }
        }

        /// <summary>
        /// Gets or sets connection.
        /// </summary>
        public IDbConnection Connection
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