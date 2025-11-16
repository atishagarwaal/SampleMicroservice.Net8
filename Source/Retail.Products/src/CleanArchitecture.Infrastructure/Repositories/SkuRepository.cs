using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;

namespace Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Product repository class.
    /// </summary>
    public class SkuRepository : GenericRepository<Sku>, ISkuRepository
    {
        private readonly ILogger<SkuRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkuRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        /// <param name="logger">Instance of logger.</param>
        public SkuRepository(ApplicationDbContext context, ILogger<SkuRepository> logger) : base(context)
        {
            _logger = logger;
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
                _logger.LogDebug("Querying SKUs by IDs. SkuIdCount: {SkuIdCount}", skuids?.Count ?? 0);
                var list = await _context.Skus.Where(i => skuids.Contains(i.Id)).ToListAsync();
                _logger.LogDebug("Retrieved {Count} SKUs from database", list.Count);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SKUs by IDs. SkuIdCount: {SkuIdCount}", skuids?.Count ?? 0);
                throw;
            }
        }
    }
}
