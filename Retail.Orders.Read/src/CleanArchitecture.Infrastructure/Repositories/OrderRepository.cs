using Microsoft.EntityFrameworkCore;
using Retail.Api.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
