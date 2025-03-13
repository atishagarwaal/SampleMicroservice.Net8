using CommonLibrary.MessageContract;
using Retail.Api.Orders.Read.src.CleanArchitecture.Application.Dto;

namespace Retail.Api.Orders.Read.src.CleanArchitecture.Application.Interfaces
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

       //Task HandleInventoryErrorEvent(InventoryErrorEvent inventoryUpdatedFailedEvent);
    }
}
