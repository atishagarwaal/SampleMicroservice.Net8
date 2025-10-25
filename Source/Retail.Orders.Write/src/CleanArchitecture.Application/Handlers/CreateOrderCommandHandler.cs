using AutoMapper;
using CommonLibrary.MessageContract;
using MediatR;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Handlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMessagePublisher _messagePublisher;

        public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IMessagePublisher messagePublisher, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
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
                
                // Ensure LineItems have proper OrderId references
                if (order.LineItems != null && order.LineItems.Any())
                {
                    foreach (var lineItem in order.LineItems)
                    {
                        Console.WriteLine($"LineItem before save - Id: {lineItem.Id}, OrderId: {lineItem.OrderId}, SkuId: {lineItem.SkuId}, Qty: {lineItem.Qty}");
                    }
                }
                
                var orderRecord = await unitOfWork.Orders.AddAsync(order);
                Console.WriteLine($"After AddAsync - Order ID: {orderRecord.Id}");
                
                // Save changes to generate the Order ID and save both Order and LineItems
                var saveResult = await unitOfWork.CompleteAsync();
                Console.WriteLine($"After CompleteAsync - SaveResult: {saveResult}, Order ID: {orderRecord.Id}");

                // Get the complete order with line items
                var savedOrder = await unitOfWork.Orders.GetByIdAsync(orderRecord.Id);
                Console.WriteLine($"Retrieved order - ID: {savedOrder?.Id}, LineItems count: {savedOrder?.LineItems?.Count ?? 0}");
                
                if (savedOrder?.LineItems != null && savedOrder.LineItems.Any())
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
                    LineItems = savedOrder.LineItems.Select(item => new CommonLibrary.Handlers.Dto.LineItemDto
                    {
                        Id = item.Id,
                        OrderId = savedOrder.Id,
                        SkuId = item.SkuId,
                        Qty = item.Qty,
                    }),
                };

                await _messagePublisher.PublishAsync(newOrderMessage, RabbitmqConstants.OrderCreated);
                await unitOfWork.CommitTransactionAsync();

                return _mapper.Map<OrderDto>(savedOrder);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
