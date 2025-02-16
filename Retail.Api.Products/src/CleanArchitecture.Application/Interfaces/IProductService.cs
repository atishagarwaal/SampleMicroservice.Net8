using Retail.Api.Products.src.CleanArchitecture.Application.Dto;

namespace Retail.Api.Products.src.CleanArchitecture.Application.Interfaces
{
    /// <summary>
    /// Interface definition for product service.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Method to fetch all products asynchronously.
        /// </summary>
        /// <returns>List of product.</returns>
        Task<IEnumerable<SkuDto>> GetAllProductsAsync();

        /// <summary>
        /// Method to fetch product record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Product object.</returns>
        Task<SkuDto> GetProductByIdAsync(long id);

        /// <summary>
        /// Method to add a new product record asynchronously.
        /// </summary>
        /// <param name="skuDto">Product record.</param>
        /// <returns>Product object.</returns>
        Task<SkuDto> AddProductAsync(SkuDto skuDto);

        /// <summary>
        /// Method to update product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <param name="skuDto">Product record.</param>
        /// <returns>Product object.</returns>
        Task<SkuDto> UpdateProductAsync(long id, SkuDto skuDto);

        /// <summary>
        /// Method to delete product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>Product object.</returns>
        Task<bool> DeleteProductAsync(long id);
    }
}
