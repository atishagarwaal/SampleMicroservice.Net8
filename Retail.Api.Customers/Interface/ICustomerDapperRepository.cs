using Retail.Api.Customers.Model;

namespace Retail.Api.Customers.Interface
{
    /// <summary>
    /// Interface definition for customer repository.
    /// </summary>
    public interface ICustomerDapperRepository
    {
        /// <summary>
        /// Add a new object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        Task<Customer> AddAsync(Customer entity);

        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object.</returns>
        Task<IEnumerable<Customer>> GetAllAsync();

        /// <summary>
        /// Get an object by Id asynchronously
        /// </summary>
        /// <param name="id">Primary key of the object.</param>
        /// <returns>Returns an object.</returns>
        Task<Customer> GetByIdAsync(long id);

        /// <summary>
        /// Removes an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        Task<int> RemoveAsync(Customer entity);

        /// <summary>
        /// Updates an object asynchronously.
        /// </summary>
        /// <param name="entity">Object parameter.</param>
        /// <returns>Returns an integer.</returns>
        Task<int> UpdateAsync(Customer entity);
    }
}
