using MediatR;

using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
namespace Retail.Orders.Write.src.CleanArchitecture.Application.Queries
{
    public class GetOrderByIdQuery : IRequest<OrderDto>
    {
        public long Id { get; set; }
    }
}
