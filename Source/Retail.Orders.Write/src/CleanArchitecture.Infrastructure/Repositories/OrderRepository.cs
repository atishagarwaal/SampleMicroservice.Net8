using Microsoft.EntityFrameworkCore;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Repositories
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

        /// <summary>
        /// Gets order by Id including LineItems
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>Returns order with LineItems.</returns>
        public override async Task<Order?> GetByIdAsync(long id)
        {
            return await _context.Orders
                .Include(o => o.LineItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
