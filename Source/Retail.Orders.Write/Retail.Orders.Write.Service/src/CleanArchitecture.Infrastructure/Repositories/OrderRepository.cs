using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Order repository class.
    /// </summary>
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        /// <param name="logger">Instance of logger.</param>
        public OrderRepository(ApplicationDbContext context, ILogger<OrderRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets order by Id including LineItems
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>Returns order with LineItems.</returns>
        public override async Task<Order?> GetByIdAsync(long id)
        {
            try
            {
                _logger.LogDebug("Retrieving order with LineItems. OrderId: {OrderId}", id);
                var order = await _context.Orders
                    .Include(o => o.LineItems)
                    .FirstOrDefaultAsync(o => o.Id == id);
                
                if (order == null)
                {
                    _logger.LogDebug("Order not found. OrderId: {OrderId}", id);
                }
                else
                {
                    _logger.LogDebug("Order retrieved successfully. OrderId: {OrderId}, LineItemsCount: {LineItemsCount}",
                        id, order.LineItems?.Count ?? 0);
                }
                
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with LineItems. OrderId: {OrderId}", id);
                throw;
            }
        }
    }
}
