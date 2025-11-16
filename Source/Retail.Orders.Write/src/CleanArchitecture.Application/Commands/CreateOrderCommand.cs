using CommonLibrary.Results;
using MediatR;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Commands
{
    /// <summary>
    /// Command to create a new order.
    /// </summary>
    public class CreateOrderCommand : IRequest<Result<OrderDto>>
    {
        /// <summary>
        /// Gets or sets the order DTO to create.
        /// </summary>
        public OrderDto Order { get; set; }
    }
}
