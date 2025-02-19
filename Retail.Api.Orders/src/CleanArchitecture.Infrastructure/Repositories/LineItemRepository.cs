using Microsoft.EntityFrameworkCore;
using Retail.Api.Orders.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class LineItemRepository : GenericRepository<LineItem>, ILineItemRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineItemRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public LineItemRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets collection of order items asynchronously.
        /// </summary>
        /// <param name="orderId">Id of object.</param>
        /// <returns>Returns collection of order items.</returns>
        public async Task<IEnumerable<LineItem>> GetOrderItemsAsync(long orderId)
        {
            var list = await _context.LineItems
            .Where(i => i.OrderId == orderId)
            .ToListAsync();

            return list;
        }
    }
}