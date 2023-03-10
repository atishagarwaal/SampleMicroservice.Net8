using Microsoft.EntityFrameworkCore;
using Retail.Api.Customers.Data;
using Retail.Api.Customers.Interface;
using Retail.Api.Customers.Repositories;

namespace Retail.Api.Customers.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private ICustomerRepository _customerRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICustomerRepository customerRepository
        { 
            get { return _customerRepository = _customerRepository ?? new CustomerRepository(_context); }
            private set { _customerRepository = value; } 
        }

        public void Commit()
        {
            _context.SaveChanges();
        }


        public async Task CommitAsync()
        { 
            await _context.SaveChangesAsync();
        }


        public void Rollback()
        {
            _context.Dispose();
        }


        public async Task RollbackAsync()
        { 
            await _context.DisposeAsync();
        }
    }
}
