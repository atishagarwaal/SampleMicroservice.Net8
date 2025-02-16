using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;

namespace Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Product repository class.
    /// </summary>
    public class ProductEntityRepository : EntityRepository<Sku>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductEntityRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public ProductEntityRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
