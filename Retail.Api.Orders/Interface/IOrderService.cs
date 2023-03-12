using Retail.Api.Orders.Dto;
using Retail.Api.Orders.Model;

namespace Retail.Api.Orders.Interface
{
    /// <summary>
    /// Interface definition for orders service.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Method to fetch all orders asynchronously.
        /// </summary>
        /// <returns>List of orders.</returns>
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

        /// <summary>
        /// Method to fetch order record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>Order object.</returns>
        Task<OrderDto> GetOrderByIdAsync(long id);

        /// <summary>
        /// Method to add a new order record asynchronously.
        /// </summary>
        /// <param name="orderDto">Order record.</param>
        /// <returns>Order object.</returns>
        Task<OrderDto> AddOrderAsync(OrderDto orderDto);

        /// <summary>
        /// Method to update order record asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <param name="orderDto">Order record.</param>
        /// <returns>Order object.</returns>
        Task<OrderDto> UpdateOrderAsync(long id, OrderDto orderDto);

        /// <summary>
        /// Method to delete order record asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>Order object.</returns>
        Task<bool> DeleteOrderAsync(long id);
    }
}
