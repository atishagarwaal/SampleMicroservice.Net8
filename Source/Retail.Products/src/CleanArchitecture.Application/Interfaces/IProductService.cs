using CommonLibrary.MessageContract;
using CommonLibrary.Results;
using OrderCreatedEventNameSpace;
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
        /// <returns>Result containing the product object if found, or an error message if not found.</returns>
        Task<Result<SkuDto>> GetProductByIdAsync(long id);

        /// <summary>
        /// Method to add a new product record asynchronously.
        /// </summary>
        /// <param name="skuDto">Product record.</param>
        /// <returns>Result containing the created product object if successful, or an error message if validation fails.</returns>
        Task<Result<SkuDto>> AddProductAsync(SkuDto skuDto);

        /// <summary>
        /// Method to update product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <param name="skuDto">Product record.</param>
        /// <returns>Result containing the updated product object if successful, or an error message if validation fails or product not found.</returns>
        Task<Result<SkuDto>> UpdateProductAsync(long id, SkuDto skuDto);

        /// <summary>
        /// Method to delete product record asynchronously.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <returns>True if product was deleted, false if not found.</returns>
        Task<bool> DeleteProductAsync(long id);

        Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent);
    }
}
