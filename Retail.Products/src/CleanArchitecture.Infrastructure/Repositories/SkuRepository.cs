using Microsoft.EntityFrameworkCore;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;

namespace Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Product repository class.
    /// </summary>
    public class SkuRepository : GenericRepository<Sku>, ISkuRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkuRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public SkuRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets collection of sku items asynchronously.
        /// </summary>
        /// <param name="skuids">Ids of objects.</param>
        /// <returns>Returns collection of sku items.</returns>
        public async Task<IEnumerable<Sku>> GetAllSkuByIdsAsync(List<long> skuids)
        {
            try
            {
                var list = await _context.Skus.Where(i => skuids.Contains(i.Id)).ToListAsync();
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
