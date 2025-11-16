using MediatR;
using Microsoft.Extensions.Logging;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Application.Queries;
using Retail.Orders.Read.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
using CommonLibrary.Results;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Handlers
{
    /// <summary>
    /// Handler for GetAllOrdersQuery.
    /// </summary>
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<IEnumerable<OrderDto>>>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConverter<Domain.Entities.Order, OrderDto> _orderDtoConverter;
        private readonly ILogger<GetAllOrdersQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllOrdersQueryHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="orderDtoConverter">Instance of order DTO converter.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="logger">Instance of logger.</param>
        public GetAllOrdersQueryHandler(
            IUnitOfWork unitOfWork,
            IConverter<Domain.Entities.Order, OrderDto> orderDtoConverter,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<GetAllOrdersQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _orderDtoConverter = orderDtoConverter;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Handles the GetAllOrdersQuery request.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result containing list of order DTOs.</returns>
        public async Task<Result<IEnumerable<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetAllOrdersQuery");
            
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var orders = await unitOfWork.Orders.GetAllAsync();
                
                if (orders == null)
                {
                    _logger.LogWarning("GetAllAsync returned null");
                    return Result<IEnumerable<OrderDto>>.Failure("Failed to retrieve orders from database");
                }

                var orderCount = orders.Count();
                _logger.LogDebug("Retrieved {OrderCount} orders from repository", orderCount);
                
                var result = orders
                    .Where(order => order != null)
                    .Select(order => _orderDtoConverter.Convert(order))
                    .ToList();
                
                var validOrderCount = result.Count;
                if (validOrderCount < orderCount)
                {
                    _logger.LogWarning("Filtered out {FilteredCount} null orders from {TotalCount} total orders", 
                        orderCount - validOrderCount, orderCount);
                }
                
                _logger.LogInformation("Successfully processed GetAllOrdersQuery. Returning {OrderCount} orders", validOrderCount);
                return Result<IEnumerable<OrderDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetAllOrdersQuery");
                return Result<IEnumerable<OrderDto>>.Failure($"Error retrieving orders: {ex.Message}");
            }
        }
    }
}
