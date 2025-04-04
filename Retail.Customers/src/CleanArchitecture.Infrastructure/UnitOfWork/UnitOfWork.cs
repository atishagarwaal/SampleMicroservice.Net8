﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Repositories;
using Retail.Customers.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Api.Customers.src.CleanArchitecture.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        public ICustomerRepository Customers { get; private set; }
        public INotificationRepository Notifications { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="entityContext">Entity framework Db context.</param>
        public UnitOfWork(ApplicationDbContext entityContext)
        {
            _context = entityContext;
            Customers = new CustomerRepository(_context);
            Notifications = new NotificationRepository(_context);
        }

        /// <summary>
        /// Method to begin transaction.
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        /// <summary>
        /// Method to commit changes.
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// Method to rollback changes.
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }
}

