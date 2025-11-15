using AutoMapper;
using CommonLibrary.MessageContract;
using MediatR;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using OrderCreatedEventNameSpace;
using Microsoft.Extensions.Logging;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Handlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IMessagePublisher messagePublisher, IServiceScopeFactory serviceScopeFactory, ILogger<CreateOrderCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await unitOfWork.BeginTransactionAsync();
            try
            {
                // Create and save the order (following Customer/Product service pattern)
                var order = _mapper.Map<Order>(request.Order);
                Console.WriteLine($"Mapped order - CustomerId: {order.CustomerId}, TotalAmount: {order.TotalAmount}");
                Console.WriteLine($"LineItems count after mapping: {order.LineItems?.Count ?? 0}");
                
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
                        Console.WriteLine($"LineItem before save - Id: {lineItem.Id}, OrderId: {lineItem.OrderId}, SkuId: {lineItem.SkuId}, Qty: {lineItem.Qty}");
                    }
                }
                
                var orderRecord = await unitOfWork.Orders.AddAsync(order);
                Console.WriteLine($"After AddAsync - Order ID: {orderRecord.Id}");
                
                // Save changes to generate the Order ID and save both Order and LineItems
                // EF Core will automatically set OrderId on LineItems via the navigation property
                var saveResult = await unitOfWork.CompleteAsync();
                Console.WriteLine($"After CompleteAsync - SaveResult: {saveResult}, Order ID: {orderRecord.Id}");

                // Get the complete order with line items
                var savedOrder = await unitOfWork.Orders.GetByIdAsync(orderRecord.Id);
                if (savedOrder == null)
                {
                    _logger.LogError("Failed to retrieve saved order with ID {OrderId}", orderRecord.Id);
                    throw new InvalidOperationException($"Order with ID {orderRecord.Id} was not found after save");
                }
                
                Console.WriteLine($"Retrieved order - ID: {savedOrder.Id}, LineItems count: {savedOrder.LineItems?.Count ?? 0}");
                
                if (savedOrder.LineItems != null && savedOrder.LineItems.Count > 0)
                {
                    foreach (var lineItem in savedOrder.LineItems)
                    {
                        Console.WriteLine($"LineItem after save - Id: {lineItem.Id}, OrderId: {lineItem.OrderId}, SkuId: {lineItem.SkuId}, Qty: {lineItem.Qty}");
                    }
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

                return _mapper.Map<OrderDto>(savedOrder);
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
