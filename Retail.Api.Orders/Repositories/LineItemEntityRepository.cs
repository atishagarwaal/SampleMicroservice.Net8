using Retail.Api.Orders.Data;
using Retail.Api.Orders.DefaultRepositories;
using Retail.Api.Orders.Interface;
using Retail.Api.Orders.Model;

namespace Retail.Api.Orders.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class LineItemEntityRepository : EntityRepository<LineItem> , ILineItemEntityRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineItemEntityRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public LineItemEntityRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
