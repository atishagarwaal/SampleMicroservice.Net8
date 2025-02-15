using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;
using Retail.Api.Orders.src.CleanArchitecture.Domain.Model;

namespace Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces
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
        Task<IEnumerable<Order>> GetAllOrdersAsync();

        /// <summary>
        /// Gets object by Id asynchronously.
        /// </summary>
        /// <param name="id">Id of object.</param>
        /// <returns>Returns object.</returns>
        Task<Order> GetOrderByIdAsync(long id);
    }
}
