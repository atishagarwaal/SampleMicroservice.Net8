using System;
using System.Threading.Tasks;
using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using Microsoft.Extensions.Logging;
using InventoryErrorEventNameSpace;
using MessagingLibrary.Interface;
using Retail.Orders.Write.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Service
{
    /// <summary>
    /// Service initializer for setting up message subscriptions.
    /// </summary>
    internal class ServiceInitializer : IServiceInitializer
    {
        private readonly IMessageSubscriber _messageSubscriber;
        private readonly IEventHandler<InventoryErrorEvent> _inventoryErrorHandler;
        private readonly ILogger<ServiceInitializer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInitializer"/> class.
        /// </summary>
        /// <param name="messageSubscriber">Instance of message subscriber.</param>
        /// <param name="inventoryErrorHandler">Instance of inventory error event handler.</param>
        /// <param name="logger">Instance of logger.</param>
        public ServiceInitializer(
            IMessageSubscriber messageSubscriber,
            IEventHandler<InventoryErrorEvent> inventoryErrorHandler,
            ILogger<ServiceInitializer> logger)
        {
            _messageSubscriber = messageSubscriber;
            _inventoryErrorHandler = inventoryErrorHandler;
            _logger = logger;
        }

        /// <summary>
        /// Initializes the service by subscribing to events.
        /// </summary>
        /// <returns>Task representing the async operation.</returns>
        public async Task Initialize()
        {
            _logger.LogInformation("Initializing Order Write Service subscriptions");
            try
            {
                await _messageSubscriber.SubscribeAsync<InventoryErrorEvent>(_inventoryErrorHandler.HandleAsync);
                _logger.LogInformation("Successfully subscribed to InventoryErrorEvent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Order Write Service subscriptions");
                throw;
            }
        }
    }
}
