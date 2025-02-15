using Microsoft.EntityFrameworkCore;
using Retail.Api.Orders.src.CleanArchitecture.Domain.Model;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class OrderEntityRepository : EntityRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderEntityRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public OrderEntityRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            var list = await _context.Orders
           .Include(o => o.LineItems)
           .ToListAsync();

            return list;
        }

        /// <summary>
        /// Gets object by Id asynchronously.
        /// </summary>
        /// <param name="id">Id of object.</param>
        /// <returns>Returns object.</returns>
        public async Task<Order> GetOrderByIdAsync(long id)
        {
            var obj = await _context.Orders
           .Include(o => o.LineItems)
           .FirstOrDefaultAsync(o => o.Id == id);

            return obj;
        }
    }
}
