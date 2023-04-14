using Retail.Api.Customers.Dto;
using Retail.Api.Customers.Model;

namespace Retail.Api.Customers.Interface
{
    /// <summary>
    /// Interface definition for customer service.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Method to fetch all customers asynchronously.
        /// </summary>
        /// <returns>List of customers.</returns>
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();

        /// <summary>
        /// Method to fetch customer record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Customer object.</returns>
        Task<CustomerDto> GetCustomerByIdAsync(long id);

        /// <summary>
        /// Method to add a new customer record asynchronously.
        /// </summary>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Customer object.</returns>
        Task<CustomerDto> AddCustomerAsync(CustomerDto custDto);

        /// <summary>
        /// Method to update customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <param name="custDto">Customer record.</param>
        /// <returns>Customer object.</returns>
        Task<CustomerDto> UpdateCustomerAsync(long id, CustomerDto custDto);

        /// <summary>
        /// Method to delete customer record asynchronously.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Customer object.</returns>
        Task<bool> DeleteCustomerAsync(long id);
    }
}
