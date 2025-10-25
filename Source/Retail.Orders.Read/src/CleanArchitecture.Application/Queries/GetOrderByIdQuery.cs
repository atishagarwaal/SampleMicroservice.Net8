using MediatR;

using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
namespace Retail.Orders.Read.src.CleanArchitecture.Application.Queries
{
    public class GetOrderByIdQuery : IRequest<OrderDto>
    {
        public long Id { get; set; }
    }
}
