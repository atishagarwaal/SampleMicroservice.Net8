using Retail.Api.Orders.Model;

namespace Retail.Api.Orders.Interface
{
    /// <summary>
    /// Interface definition for customer repository.
    /// </summary>
    public interface ILineItemDapperRepository
    {
        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        Task<int> AddAsync(LineItem entity);

        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object.</returns>
        Task<IEnumerable<LineItem>> GetAllAsync();

        /// <summary>
        /// Get an object by Id asynchronously
        /// </summary>
        /// <param name="id">Primary key of the object.</param>
        /// <returns>Returns an object.</returns>
        Task<LineItem> GetByIdAsync(long id);

        /// <summary>
        /// Removes an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        Task<int> RemoveAsync(LineItem entity);

        /// <summary>
        /// Updates an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        Task<int> UpdateAsync(LineItem entity);
    }
}
