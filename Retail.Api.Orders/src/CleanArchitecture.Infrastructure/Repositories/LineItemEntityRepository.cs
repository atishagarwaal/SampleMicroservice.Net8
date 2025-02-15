using Retail.Api.Orders.src.CleanArchitecture.Domain.Model;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Data;

namespace Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class LineItemEntityRepository : EntityRepository<LineItem>
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