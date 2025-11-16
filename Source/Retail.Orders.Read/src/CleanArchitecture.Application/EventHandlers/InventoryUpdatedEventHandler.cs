namespace Retail.Orders.Read.src.CleanArchitecture.Application.EventHandlers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CommonLibrary.Handlers;
    using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
    using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using InventoryUpdatedEventNameSpace;

    /// <summary>
    /// Event handler for InventoryUpdatedEvent.
    /// </summary>
    public class InventoryUpdatedEventHandler : IEventHandler<InventoryUpdatedEvent>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InventoryUpdatedEventHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryUpdatedEventHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="logger">Instance of logger.</param>
        public InventoryUpdatedEventHandler(
            IUnitOfWork unitOfWork,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<InventoryUpdatedEventHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task HandleAsync(InventoryUpdatedEvent inventoryUpdatedEvent)
        {
            if (inventoryUpdatedEvent == null)
            {
                _logger.LogError("InventoryUpdatedEvent is null");
                throw new ArgumentNullException(nameof(inventoryUpdatedEvent));
            }

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

                // Ensure LineItems are properly included
                var lineItems = inventoryUpdatedEvent.LineItems?
                    .Where(li => li != null)
                    .Select(dto => new Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.LineItem
                    {
                        Id = dto.Id > 0 ? dto.Id : 0, // MongoDB will generate if needed
                        OrderId = inventoryUpdatedEvent.OrderId, // Ensure OrderId is set correctly
                        SkuId = dto.SkuId,
                        Qty = (int)dto.Qty
                    })
                    .ToList() ?? new List<Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.LineItem>();

                var order = new Order
                {
                    Id = inventoryUpdatedEvent.OrderId,
                    CustomerId = inventoryUpdatedEvent.CustomerId,
                    OrderDate = inventoryUpdatedEvent.OrderDate.DateTime,
                    LineItems = lineItems,
                    TotalAmount = inventoryUpdatedEvent.TotalAmount,
                };

                _logger.LogInformation("Adding order {OrderId} to read model with {LineItemCount} line items", 
                    inventoryUpdatedEvent.OrderId, lineItems.Count);

                await unitOfWork.Orders.AddAsync(order);
                _logger.LogInformation("Order {OrderId} successfully added to read model with {LineItemCount} line items", 
                    inventoryUpdatedEvent.OrderId, lineItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing InventoryUpdatedEvent for OrderId {OrderId}", inventoryUpdatedEvent.OrderId);
                throw;
            }
        }
    }
}
