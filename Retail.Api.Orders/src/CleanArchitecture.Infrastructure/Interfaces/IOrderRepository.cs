using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;
using Retail.Api.Orders.src.CleanArchitecture.Domain.Entities;

namespace Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface definition for orders repository.
    /// </summary>
    public interface IOrderRepository : IGenericRepository<Order>
    {
    }
}
