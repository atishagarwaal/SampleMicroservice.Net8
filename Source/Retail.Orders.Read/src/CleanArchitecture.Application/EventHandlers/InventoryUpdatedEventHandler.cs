using AutoMapper;
using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.EventHandlers
{
    public class InventoryUpdatedEventHandler : IEventHandler<InventoryUpdatedEvent>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryUpdatedEventHandler> _logger;

        public InventoryUpdatedEventHandler(IUnitOfWork unitOfWork, IMapper mapper, IServiceScopeFactory serviceScopeFactory, ILogger<InventoryUpdatedEventHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task HandleAsync(InventoryUpdatedEvent inventoryUpdatedEvent)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // Check if order already exists to prevent duplicate key errors
                var existingOrder = await unitOfWork.Orders.GetByIdAsync(inventoryUpdatedEvent.OrderId);
                if (existingOrder != null)
                {
                    _logger.LogInformation("Order {OrderId} already exists in read model, skipping insertion", inventoryUpdatedEvent.OrderId);
                    return;
                }

                var order = new Order
                {
                    Id = inventoryUpdatedEvent.OrderId,
                    CustomerId = inventoryUpdatedEvent.CustomerId,
                    OrderDate = inventoryUpdatedEvent.OrderDate,
                    LineItems = inventoryUpdatedEvent.LineItems
                                .Select(dto => new LineItem
                                {
                                    Id = dto.Id,
                                    OrderId = dto.OrderId,
                                    SkuId = dto.SkuId,
                                    Qty = dto.Qty
                                })
                                .ToList(),
                    TotalAmount = inventoryUpdatedEvent.TotalAmount,
                };

                await unitOfWork.Orders.AddAsync(order);
                _logger.LogInformation("Order {OrderId} successfully added to read model", inventoryUpdatedEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing InventoryUpdatedEvent for OrderId {OrderId}", inventoryUpdatedEvent.OrderId);
                throw;
            }
        }
    }
}
