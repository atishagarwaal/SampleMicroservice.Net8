using CommonLibrary.MessageContract;
using MediatR;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;
using OrderCreatedEventNameSpace;
using Microsoft.Extensions.Logging;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Handlers
{
    /// <summary>
    /// Handler for CreateOrderCommand.
    /// </summary>
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConverter<OrderDto, Order> _orderConverter;
        private readonly IConverter<Order, OrderDto> _orderDtoConverter;
        private readonly IMessageValidator<OrderDto> _orderDtoValidator;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOrderCommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="orderConverter">Instance of order converter.</param>
        /// <param name="orderDtoConverter">Instance of order DTO converter.</param>
        /// <param name="orderDtoValidator">Instance of order DTO validator.</param>
        /// <param name="messagePublisher">Instance of message publisher.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="logger">Instance of logger.</param>
        public CreateOrderCommandHandler(
            IUnitOfWork unitOfWork,
            IConverter<OrderDto, Order> orderConverter,
            IConverter<Order, OrderDto> orderDtoConverter,
            IMessageValidator<OrderDto> orderDtoValidator,
            IMessagePublisher messagePublisher,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _orderConverter = orderConverter;
            _orderDtoConverter = orderDtoConverter;
            _orderDtoValidator = orderDtoValidator;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Handles the CreateOrderCommand request.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Created order DTO.</returns>
        public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await unitOfWork.BeginTransactionAsync();
            try
            {
                // Validate order DTO
                var validationResult = _orderDtoValidator.Validate(request.Order);
                if (!validationResult.IsValid)
                {
                    throw new ArgumentException(validationResult.FailureReason ?? "Validation failed", nameof(request.Order));
                }

                // Convert DTO to entity
                var order = _orderConverter.Convert(request.Order);
                
                // Ensure LineItems are properly configured for new entities
                // EF Core will automatically set OrderId when Order is saved due to navigation property
                if (order.LineItems != null && order.LineItems.Any())
                {
                    foreach (var lineItem in order.LineItems)
                    {
                        // Reset Id to 0 for new LineItems (EF will generate it)
                        lineItem.Id = 0;
                        // OrderId will be set automatically by EF Core via navigation property when Order is saved
                        // But we need to ensure the navigation property is set
                        lineItem.Order = order;
                    }
                }
                
                var orderRecord = await unitOfWork.Orders.AddAsync(order);
                
                // Save changes to generate the Order ID and save both Order and LineItems
                // EF Core will automatically set OrderId on LineItems via the navigation property
                await unitOfWork.CompleteAsync();

                // Get the complete order with line items
                var savedOrder = await unitOfWork.Orders.GetByIdAsync(orderRecord.Id);
                if (savedOrder == null)
                {
                    _logger.LogError("Failed to retrieve saved order with ID {OrderId}", orderRecord.Id);
                    throw new InvalidOperationException($"Order with ID {orderRecord.Id} was not found after save");
                }

                // Create and publish the event
                var newOrderMessage = new OrderCreatedEvent
                {
                    CustomerId = savedOrder.CustomerId,
                    OrderDate = savedOrder.OrderDate,
                    TotalAmount = savedOrder.TotalAmount,
                    OrderId = savedOrder.Id,
                    LineItems = savedOrder.LineItems?
                        .Select(item => new OrderCreatedEventNameSpace.LineItem
                        {
                            Id = item.Id,
                            OrderId = savedOrder.Id,
                            SkuId = item.SkuId,
                            Qty = item.Qty,
                        })
                        .ToArray() ?? Array.Empty<OrderCreatedEventNameSpace.LineItem>(),
                };

                await _messagePublisher.PublishAsync(newOrderMessage, RabbitmqConstants.OrderCreated);
                await unitOfWork.CommitTransactionAsync();

                return _orderDtoConverter.Convert(savedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order. CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, LineItemsCount: {LineItemsCount}", 
                    request.Order?.CustomerId, request.Order?.TotalAmount, request.Order?.LineItems?.Count ?? 0);
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
