using Retail.Api.Customers.Dto;

namespace Retail.Api.Customers.Interface
{
    /// <summary>
    /// Interface definition for customer service.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Method to fetch customer record based on Id.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <returns>Customer object.</returns>
        CustomerDto GetCustomerById(long id);
    }
}
