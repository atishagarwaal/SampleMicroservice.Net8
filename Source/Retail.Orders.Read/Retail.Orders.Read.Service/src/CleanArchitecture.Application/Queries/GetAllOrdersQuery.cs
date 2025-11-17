using MediatR;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using CommonLibrary.Results;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Queries
{
    /// <summary>
    /// Query to retrieve all orders.
    /// </summary>
    public class GetAllOrdersQuery : IRequest<Result<IEnumerable<OrderDto>>>
    {
    }
}
