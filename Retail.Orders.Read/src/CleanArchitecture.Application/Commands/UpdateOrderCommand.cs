using MediatR;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Commands
{
    public class UpdateOrderCommand : IRequest<OrderDto>
    {
        public OrderDto Order { get; set; }
    }
}
