using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Retail.Api.Orders.CustomInterface;
using Retail.Api.Orders.Data;
using Retail.Api.Orders.DefaultInterface;
using Retail.Api.Orders.Model;
using Retail.Api.Orders.Repositories;
using System.Data;

namespace Retail.Api.Orders.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class DapperUnitOfWork : IUnitOfWork
    {
        private readonly DapperContext _dapperContext;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private IOrderRepository? _orderRepository;
        private IRepository<LineItem>? _lineItemRepository;

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
        /// Gets or sets customer repository.
        /// </summary>
        public IOrderRepository OrderRepository
        {
            get
            {
                if (_orderRepository == null)
                {
                    _orderRepository = new OrderDapperRepository(_dapperContext);
                }

                return _orderRepository;
            }
        }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        public IRepository<LineItem> LineItemRepository
        {
            get
            {
                if (_lineItemRepository == null)
                {
                    _lineItemRepository = new LineItemDapperRepository(_dapperContext);
                }

                return _lineItemRepository;
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