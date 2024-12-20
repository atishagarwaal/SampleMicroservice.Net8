using Microsoft.AspNetCore.Mvc;
using Retail.BFFWeb.Api.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.BFFWeb.Api.Interface
{
    /// <summary>
    /// Product provider interface.
    /// </summary>
    public interface IProductProvider
    {
        /// <summary>
        /// Method to return list of all products.
        /// </summary>
        /// <returns>List of products.</returns>
        Task<IEnumerable<SkuDto>> GetAllProductsAsync();

        /// <summary>
        /// Method to fetch product record based on Id.
        /// </summary>
        /// <returns>Product object.</returns>
        Task<SkuDto> GetProductByIdAsync(long id);
    }
}
