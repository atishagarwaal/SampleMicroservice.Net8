namespace Retail.Api.Customers.src.CleanArchitecture.Application.EventHandlers
{
    using System;
    using CommonLibrary.Handlers;
    using InventoryUpdatedEventNameSpace;
    using MessagingLibrary.Interface;
    using Microsoft.Extensions.Logging;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;

    /// <summary>
    /// Event handler for InventoryUpdatedEvent.
    /// </summary>
    public class InventoryUpdatedEventHandler : IEventHandler<InventoryUpdatedEvent>
    {
        private readonly ICustomerService _customerService;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<InventoryUpdatedEventHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryUpdatedEventHandler"/> class.
        /// </summary>
        /// <param name="customerService">Instance of customer service.</param>
        /// <param name="messagePublisher">Instance of message publisher.</param>
        /// <param name="logger">Instance of logger.</param>
        public InventoryUpdatedEventHandler(
            ICustomerService customerService,
            IMessagePublisher messagePublisher,
            ILogger<InventoryUpdatedEventHandler> logger)
        {
            _customerService = customerService;
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        /// <summary>
        /// Handles the InventoryUpdatedEvent asynchronously.
        /// </summary>
        /// <param name="inventoryUpdatedEvent">Inventory updated event.</param>
        public async Task HandleAsync(InventoryUpdatedEvent inventoryUpdatedEvent)
        {
            if (inventoryUpdatedEvent == null)
            {
                _logger.LogError("InventoryUpdatedEvent is null");
                throw new ArgumentNullException(nameof(inventoryUpdatedEvent));
            }

            _logger.LogInformation("Received InventoryUpdatedEvent. OrderId: {OrderId}, CustomerId: {CustomerId}", 
                inventoryUpdatedEvent.OrderId, inventoryUpdatedEvent.CustomerId);
            
            try
            {
                await _customerService.HandleOrderCreatedEvent(inventoryUpdatedEvent);
                _logger.LogInformation("Successfully processed InventoryUpdatedEvent for OrderId: {OrderId}", 
                    inventoryUpdatedEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing InventoryUpdatedEvent. OrderId: {OrderId}, CustomerId: {CustomerId}", 
                    inventoryUpdatedEvent.OrderId, inventoryUpdatedEvent.CustomerId);
                throw;
            }
        }
    }
}
