using MediatR;
using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.src.CleanArchitecture.Application.Commands
{
    public class CreateOrderCommand : IRequest<OrderDto>
    {
        public OrderDto Order { get; set; }
    }
}
