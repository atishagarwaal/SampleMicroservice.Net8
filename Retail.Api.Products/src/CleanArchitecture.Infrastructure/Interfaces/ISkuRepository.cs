using Microsoft.EntityFrameworkCore;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Data;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Api.Products.src.CleanArchitecture.Infrastructure.Repositories
{
    /// <summary>
    /// Product repository class.
    /// </summary>
    public interface ISkuRepository : IGenericRepository<Sku>
    {
        /// <summary>
        /// Gets collection of sku items asynchronously.
        /// </summary>
        /// <param name="skuids">Ids of objects.</param>
        /// <returns>Returns collection of sku items.</returns>
        Task<IEnumerable<Sku>> GetAllSkuByIdsAsync(List<long> skuids);
    }
}
