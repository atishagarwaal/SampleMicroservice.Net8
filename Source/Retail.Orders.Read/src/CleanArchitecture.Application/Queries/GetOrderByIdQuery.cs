using MediatR;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using CommonLibrary.Results;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Queries
{
    /// <summary>
    /// Query to retrieve an order by ID.
    /// </summary>
    public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        public long Id { get; set; }
    }
}
