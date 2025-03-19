using MediatR;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Queries
{
    public class GetAllOrdersQuery : IRequest<IEnumerable<OrderDto>>
    {
    }
}
