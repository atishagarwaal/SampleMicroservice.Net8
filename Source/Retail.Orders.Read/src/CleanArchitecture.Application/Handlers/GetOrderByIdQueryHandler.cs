using MediatR;
using Microsoft.Extensions.Logging;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Application.Queries;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Handlers
{
    /// <summary>
    /// Handler for GetOrderByIdQuery.
    /// </summary>
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConverter<Domain.Entities.Order, OrderDto> _orderDtoConverter;
        private readonly ILogger<GetOrderByIdQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOrderByIdQueryHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="orderDtoConverter">Instance of order DTO converter.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="logger">Instance of logger.</param>
        public GetOrderByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IConverter<Domain.Entities.Order, OrderDto> orderDtoConverter,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<GetOrderByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _orderDtoConverter = orderDtoConverter;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetOrderByIdQuery request.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Order DTO.</returns>
        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetOrderByIdQuery for OrderId {OrderId}", request.Id);
            
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var order = await unitOfWork.Orders.GetByIdAsync(request.Id);
                
                if (order == null)
                {
                    _logger.LogWarning("Order with Id {OrderId} not found in repository", request.Id);
                    return null!;
                }

                _logger.LogDebug("Order with Id {OrderId} found. Converting to DTO", request.Id);
                var result = _orderDtoConverter.Convert(order);
                
                _logger.LogInformation("Successfully processed GetOrderByIdQuery for OrderId {OrderId}", request.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetOrderByIdQuery for OrderId {OrderId}", request.Id);
                throw;
            }
        }
    }
}
