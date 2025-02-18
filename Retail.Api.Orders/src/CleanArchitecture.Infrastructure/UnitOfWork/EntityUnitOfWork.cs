using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Retail.Api.Orders.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Repositories;

namespace Retail.Api.Orders.src.CleanArchitecture.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class EntityUnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _entityContext;
        private IDbContextTransaction? _entityTransaction;
        private IOrderRepository? _orderRepository;
        private ILineItemRepository? _lineItemRepository;

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
        public IOrderRepository OrderRepository
        {
            get
            {
                if (_orderRepository == null)
                {
                    _orderRepository = new OrderEntityRepository(_entityContext);
                }

                return _orderRepository;
            }
        }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        public ILineItemRepository LineItemRepository
        {
            get
            {
                if (_lineItemRepository == null)
                {
                    _lineItemRepository = new LineItemEntityRepository(_entityContext);
                }

                return _lineItemRepository;
            }
        }

        /// <summary>
        /// Method to begin transaction.
        /// </summary>
        public void BeginTransaction()
        {
            using var transaction = _entityContext.Database.BeginTransactionAsync();
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
        }
    }
}
