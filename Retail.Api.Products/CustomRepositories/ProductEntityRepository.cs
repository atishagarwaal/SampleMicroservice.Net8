using Retail.Api.Products.Data;
using Retail.Api.Products.DefaultRepositories;
using Retail.Api.Products.Interface;
using Retail.Api.Products.Model;

namespace Retail.Api.Products.Repositories
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
