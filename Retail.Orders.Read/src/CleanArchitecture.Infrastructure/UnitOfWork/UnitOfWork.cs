using MongoDB.Driver;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Repositories;
using System.Data;

namespace Retail.Orders.Read.src.CleanArchitecture.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unit of work class.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IOrderRepository Orders { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Orders = new OrderRepository(context);
        }
    }
}