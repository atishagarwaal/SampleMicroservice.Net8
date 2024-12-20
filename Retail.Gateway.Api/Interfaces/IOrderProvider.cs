using Microsoft.AspNetCore.Mvc;
using Retail.BFFWeb.Api.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.BFFWeb.Api.Interface
{
    /// <summary>
    /// Order provider interface.
    /// </summary>
    public interface IOrderProvider
    {
        /// <summary>
        /// Method to return list of all orders.
        /// </summary>
        /// <returns>List of orders.</returns>
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

        /// <summary>
        /// Method to fetch order record based on Id.
        /// </summary>
        /// <returns>Order object.</returns>
        Task<OrderDto> GetOrderByIdAsync(long id);
    }
}
