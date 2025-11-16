using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using Microsoft.Extensions.Logging;
using MessagingLibrary.Interface;
using OrderCreatedEventNameSpace;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Service
{
    /// <summary>
    /// Service initializer for setting up message subscriptions.
    /// </summary>
    internal class ServiceInitializer : IServiceInitializer
    {
        private readonly IMessageSubscriber _messageSubscriber;
        private readonly IEventHandler<OrderCreatedEvent> _orderCreatedHandler;
        private readonly ILogger<ServiceInitializer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceInitializer"/> class.
        /// </summary>
        /// <param name="messageSubscriber">Instance of message subscriber.</param>
        /// <param name="orderCreatedHandler">Instance of order created event handler.</param>
        /// <param name="logger">Instance of logger.</param>
        public ServiceInitializer(
            IMessageSubscriber messageSubscriber,
            IEventHandler<OrderCreatedEvent> orderCreatedHandler,
            ILogger<ServiceInitializer> logger)
        {
            _messageSubscriber = messageSubscriber;
            _orderCreatedHandler = orderCreatedHandler;
            _logger = logger;
        }

        /// <summary>
        /// Initializes the service by subscribing to events.
        /// </summary>
        /// <returns>Task representing the async operation.</returns>
        public async Task Initialize()
        {
            _logger.LogInformation("Initializing Product Service subscriptions");
            try
            {
                await _messageSubscriber.SubscribeAsync<OrderCreatedEvent>(_orderCreatedHandler.HandleAsync);
                _logger.LogInformation("Successfully subscribed to OrderCreatedEvent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Product Service subscriptions");
                throw;
            }
        }
    }
}
