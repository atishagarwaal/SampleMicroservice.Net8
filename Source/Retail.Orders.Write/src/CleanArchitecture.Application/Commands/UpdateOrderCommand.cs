using CommonLibrary.Results;
using MediatR;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Commands
{
    /// <summary>
    /// Command to update an existing order.
    /// </summary>
    public class UpdateOrderCommand : IRequest<Result<OrderDto>>
    {
        /// <summary>
        /// Gets or sets the order DTO to update.
        /// </summary>
        public OrderDto Order { get; set; }
    }
}
