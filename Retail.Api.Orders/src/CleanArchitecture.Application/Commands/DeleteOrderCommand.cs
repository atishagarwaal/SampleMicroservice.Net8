using MediatR;

namespace Retail.Orders.src.CleanArchitecture.Application.Commands
{
    public class DeleteOrderCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }
}
