using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Repositories;

namespace Retail.Api.Customers.src.CleanArchitecture.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    internal class EntityUnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _entityContext;
        private IDbContextTransaction? _entityTransaction;
        private IRepository<Customer>? _customerRepository;
        private IRepository<Notification>? _notificationRepository;

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
        public IRepository<Customer> CustomerRepository
        {
            get
            {
                if (_customerRepository == null)
                {
                    _customerRepository = new CustomerRepository(_entityContext);
                }

                return _customerRepository;
            }
        }

        /// <summary>
        /// Gets or sets notification repository.
        /// </summary>
        public IRepository<Notification> NotificationRepository
        {
            get
            {
                if (_notificationRepository == null)
                {
                    _notificationRepository = new NotificationRepository(_entityContext);
                }

                return _notificationRepository;
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
