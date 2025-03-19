using Microsoft.EntityFrameworkCore;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Data;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Repositories
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
    }
}