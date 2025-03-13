using MediatR;

using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;
namespace Retail.Orders.src.CleanArchitecture.Application.Queries
{
    public class GetOrderByIdQuery : IRequest<OrderDto>
    {
        public long Id { get; set; }
    }
}
