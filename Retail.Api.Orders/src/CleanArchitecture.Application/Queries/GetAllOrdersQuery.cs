using MediatR;
using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.src.CleanArchitecture.Application.Queries
{
    public class GetAllOrdersQuery : IRequest<IEnumerable<OrderDto>>
    {
    }
}
