namespace Retail.Api.Orders.src.CleanArchitecture.Application.Service
{
    using AutoMapper;
    using CommonLibrary.MessageContract;
    using MessagingInfrastructure;
    using MessagingLibrary.Interface;
    using Microsoft.Extensions.DependencyInjection;
    using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Orders.src.CleanArchitecture.Application.Interfaces;
    using Retail.Api.Orders.src.CleanArchitecture.Domain.Entities;
    using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces;
    using static NuGet.Packaging.PackagingConstants;

    /// <summary>
    /// Order service class.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderService"/> class.
        /// </summary>
        /// <param name="unitOfWork">Intance of unit of work class.</param>
        /// <param name="mapper">Intance of mapper class.</param>
        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IMessagePublisher messagePublisher, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Method to fetch all orders asynchronously.
        /// </summary>
        /// <returns>List of orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        /// <summary>
        /// Method to fetch order record based on Id asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>Order object.</returns>
        public async Task<OrderDto> GetOrderByIdAsync(long id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            return _mapper.Map<OrderDto>(order);
        }

        /// <summary>
        /// Method to add a new order record asynchronously.
        /// </summary>
        /// <param name="orderDto">Order record.</param>
        /// <returns>Order object.</returns>
        public async Task<OrderDto> AddOrderAsync(OrderDto orderDto)
        {
            try
            {
                // Start transaction
                await _unitOfWork.BeginTransactionAsync();

                var order = _mapper.Map<Order>(orderDto);
                var orderRecord = await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.CompleteAsync();

                // Publish event only after successful database operation
                var newOrderMessage = new OrderCreatedEvent
                {
                    CustomerId = orderRecord.CustomerId,
                    OrderDate = orderRecord.OrderDate,
                    TotalAmount = orderRecord.TotalAmount,
                    OrderId = orderRecord.Id,
                    LineItems = orderRecord.LineItems.Select(item => new CommonLibrary.Handlers.Dto.LineItemDto()
                    {
                        Id = item.Id,
                        OrderId = orderRecord.Id,
                        SkuId = item.SkuId,
                        Qty = item.Qty,
                    }),
                };

                await _messagePublisher.PublishAsync(newOrderMessage, RabbitmqConstants.OrderCreated);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<OrderDto>(orderRecord);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Method to update order record asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <param name="orderDto">Order record.</param>
        /// <returns>Order object.</returns>
        public async Task<OrderDto> UpdateOrderAsync(long id, OrderDto orderDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = _mapper.Map<Order>(orderDto);
                _unitOfWork.Orders.Update(order);

                if (orderDto.LineItems != null)
                {
                    foreach (var lineItem in orderDto.LineItems)
                    {
                        var lineRecord = _mapper.Map<LineItem>(lineItem);
                        _unitOfWork.LineItems.Update(lineRecord);
                    }
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                var updatedOrder = await _unitOfWork.Orders.GetByIdAsync(id);
                return _mapper.Map<OrderDto>(updatedOrder);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Method to delete order record asynchronously.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>Order object.</returns>
        public async Task<bool> RemoveOrderAsync(long id)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(id);
                if (order == null)
                    return false;

                await _unitOfWork.BeginTransactionAsync();

                _unitOfWork.Orders.Remove(order);
                if (order.LineItems != null)
                {
                    foreach (var lineItem in order.LineItems)
                    {
                        _unitOfWork.LineItems.Remove(lineItem);
                    }
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task HandleInventoryErrorEvent(InventoryErrorEvent inventoryUpdatedFailedEvent)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var order = await unitOfWork.Orders.GetByIdAsync(inventoryUpdatedFailedEvent.OrderId);
                if (order == null)
                    throw new Exception("Order does not exist");

                await unitOfWork.BeginTransactionAsync();
                unitOfWork.Orders.Remove(order);
                await unitOfWork.CompleteAsync();
                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}