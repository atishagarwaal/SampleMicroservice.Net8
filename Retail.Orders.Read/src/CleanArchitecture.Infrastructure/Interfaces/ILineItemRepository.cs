using Retail.Api.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Api.Orders.Read.src.CleanArchitecture.Domain.Entities;

namespace Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface definition for orders repository.
    /// </summary>
    public interface ILineItemRepository : IGenericRepository<LineItem>
    {
    }
}
