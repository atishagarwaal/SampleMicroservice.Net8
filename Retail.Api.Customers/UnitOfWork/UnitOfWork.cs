using Microsoft.EntityFrameworkCore;
using Retail.Api.Customers.Data;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Repositories;

namespace Retail.Api.Customers.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private ICustomerRepository? _customerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets or sets customer repository.
        /// </summary>
        public ICustomerRepository CustomerRepository
        {
            get { return _customerRepository = _customerRepository ?? new CustomerRepository(_context); }
        }

        /// <summary>
        /// Method to commit changes.
        /// </summary>
        public void Commit()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Method to commit changes asynchronously.
        /// </summary>
        public async Task CommitAsync()
        { 
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Method to rollback changes.
        /// </summary>
        public void Rollback()
        {
            _context.Dispose();
        }

        /// <summary>
        /// Method to rollback changes asynchronously.
        /// </summary>
        public async Task RollbackAsync()
        { 
            await _context.DisposeAsync();
        }
    }
}
