using Retail.Api.Orders.DefaultInterface;
using Retail.Api.Orders.Dto;
using Retail.Api.Orders.Model;

namespace Retail.Api.Orders.CustomInterface
{
    /// <summary>
    /// Interface definition for orders repository.
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

        /// <summary>
        /// Gets object by Id asynchronously.
        /// </summary>
        /// <param name="id">Id of object.</param>
        /// <returns>Returns object.</returns>
        Task<OrderDto?> GetOrderByIdAsync(long id);
    }
}
