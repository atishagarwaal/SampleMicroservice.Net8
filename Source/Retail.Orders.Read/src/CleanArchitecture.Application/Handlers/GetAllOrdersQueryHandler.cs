using MediatR;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Application.Queries;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Handlers
{
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDto>>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConverter<Domain.Entities.Order, OrderDto> _orderDtoConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllOrdersQueryHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="orderDtoConverter">Instance of order DTO converter.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        public GetAllOrdersQueryHandler(
            IUnitOfWork unitOfWork,
            IConverter<Domain.Entities.Order, OrderDto> orderDtoConverter,
            IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _orderDtoConverter = orderDtoConverter;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Handles the GetAllOrdersQuery request.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of order DTOs.</returns>
        public async Task<IEnumerable<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var orders = await unitOfWork.Orders.GetAllAsync();
            return orders
                .Where(order => order != null)
                .Select(order => _orderDtoConverter.Convert(order));
        }
    }
}
