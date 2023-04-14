using Retail.Api.Orders.DefaultInterface;
using Retail.Api.Orders.Model;
using System.Linq.Expressions;

namespace Retail.Api.Orders.Interface
{
    /// <summary>
    /// Interface definition for customer repository.
    /// </summary>
    public interface IOrderEntityRepository : IEntityRepository<Order>
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
        Task<Order?> GetOrderByIdAsync(long id);
    }
}
