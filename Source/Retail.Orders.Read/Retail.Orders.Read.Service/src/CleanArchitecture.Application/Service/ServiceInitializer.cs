namespace Retail.Orders.Read.src.CleanArchitecture.Application.Service
{
    using CommonLibrary.Handlers;
    using InventoryUpdatedEventNameSpace;
    using MessagingLibrary.Interface;
    using Microsoft.Extensions.Logging;
    using Retail.Orders.Read.src.CleanArchitecture.Application.Interfaces;

    /// <summary>
    /// Service initializer for setting up message subscriptions.
    /// </summary>
    internal class ServiceInitializer : IServiceInitializer
    {
        private readonly IMessageSubscriber _messageSubscriber;
        private readonly IEventHandler<InventoryUpdatedEvent> _inventoryUpdatedHandler;
        private readonly ILogger<ServiceInitializer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInitializer"/> class.
        /// </summary>
        /// <param name="messageSubscriber">Instance of message subscriber.</param>
        /// <param name="orderCreatedHandler">Instance of inventory updated event handler.</param>
        /// <param name="logger">Instance of logger.</param>
        public ServiceInitializer(
            IMessageSubscriber messageSubscriber,
            IEventHandler<InventoryUpdatedEvent> orderCreatedHandler,
            ILogger<ServiceInitializer> logger)
        {
            _messageSubscriber = messageSubscriber;
            _inventoryUpdatedHandler = orderCreatedHandler;
            _logger = logger;
        }

        /// <summary>
        /// Initializes the service by subscribing to events.
        /// </summary>
        /// <returns>Task representing the async operation.</returns>
        public async Task Initialize()
        {
            _logger.LogInformation("Initializing Order Read Service subscriptions");
            try
            {
                await _messageSubscriber.SubscribeAsync<InventoryUpdatedEvent>(_inventoryUpdatedHandler.HandleAsync);
                _logger.LogInformation("Successfully subscribed to InventoryUpdatedEvent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Order Read Service subscriptions");
                throw;
            }
        }
    }
}
