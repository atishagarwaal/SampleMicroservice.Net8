using MediatR;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Commands
{
    public class CreateOrderCommand : IRequest<OrderDto>
    {
        public OrderDto Order { get; set; }
    }
}
