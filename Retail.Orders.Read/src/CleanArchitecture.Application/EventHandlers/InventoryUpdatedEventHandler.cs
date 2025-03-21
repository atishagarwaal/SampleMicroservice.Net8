using AutoMapper;
using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.EventHandlers
{
    public class InventoryUpdatedEventHandler : IEventHandler<InventoryUpdatedEvent>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InventoryUpdatedEventHandler(IUnitOfWork unitOfWork, IMapper mapper, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task HandleAsync(InventoryUpdatedEvent inventoryUpdatedEvent)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var order = new Order
            {
                Id = inventoryUpdatedEvent.OrderId,
                CustomerId = inventoryUpdatedEvent.CustomerId,
                OrderDate = DateTime.UtcNow,
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
        }
    }
}
