using Microsoft.AspNetCore.Mvc;
using Retail.BFFWeb.Api.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.BFFWeb.Api.Interface
{
    /// <summary>
    /// Customer provider interface.
    /// </summary>
    public interface ICustomerProvider
    {
        /// <summary>
        /// Method to return list of all customers.
        /// </summary>
        /// <returns>List of customers.</returns>
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();


        /// <summary>
        /// Method to fetch customer record based on Id.
        /// </summary>
        /// <returns>Customer object.</returns>
        Task<CustomerDto> GetCustomerByIdAsync(long id);
    }
}
