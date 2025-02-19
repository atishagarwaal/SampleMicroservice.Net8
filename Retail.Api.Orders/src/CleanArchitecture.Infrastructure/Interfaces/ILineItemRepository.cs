using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;
using Retail.Api.Orders.src.CleanArchitecture.Domain.Entities;

namespace Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface definition for orders repository.
    /// </summary>
    public interface ILineItemRepository : IGenericRepository<LineItem>
    {
        /// <summary>
        /// Gets collection of order items asynchronously.
        /// </summary>
        /// <param name="orderId">Id of object.</param>
        /// <returns>Returns collection of order items.</returns>
        Task<IEnumerable<LineItem>> GetOrderItemsAsync(long orderId);
    }
}
