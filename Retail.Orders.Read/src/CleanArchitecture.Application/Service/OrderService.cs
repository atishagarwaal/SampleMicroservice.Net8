namespace Retail.Api.Orders.Read.src.CleanArchitecture.Application.Service
{
    using AutoMapper;
    using CommonLibrary.MessageContract;
    using MessagingInfrastructure;
    using MessagingLibrary.Interface;
    using Microsoft.Extensions.DependencyInjection;
    using Retail.Api.Orders.Read.src.CleanArchitecture.Application.Dto;
    using Retail.Api.Orders.Read.src.CleanArchitecture.Application.Interfaces;
    using Retail.Api.Orders.Read.src.CleanArchitecture.Domain.Entities;
    using Retail.Api.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
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

        ////public async Task HandleInventoryErrorEvent(InventoryErrorEvent inventoryUpdatedFailedEvent)
        ////{
        ////    try
        ////    {
        ////        using var scope = _serviceScopeFactory.CreateScope();
        ////        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        ////        var order = await unitOfWork.Orders.GetByIdAsync(inventoryUpdatedFailedEvent.OrderId);
        ////        if (order == null)
        ////            throw new Exception("Order does not exist");

        ////        await unitOfWork.BeginTransactionAsync();
        ////        unitOfWork.Orders.Remove(order);
        ////        await unitOfWork.CompleteAsync();
        ////        await unitOfWork.CommitTransactionAsync();
        ////    }
        ////    catch
        ////    {
        ////        await _unitOfWork.RollbackTransactionAsync();
        ////        throw;
        ////    }
        ////}
    }
}