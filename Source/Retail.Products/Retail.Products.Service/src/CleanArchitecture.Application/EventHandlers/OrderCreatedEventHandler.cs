using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using Microsoft.Extensions.Logging;
using OrderCreatedEventNameSpace;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Api.Products.src.CleanArchitecture.Application.EventHandlers
{
    /// <summary>
    /// Event handler for OrderCreatedEvent.
    /// </summary>
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly IProductService _productService;
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderCreatedEventHandler"/> class.
        /// </summary>
        /// <param name="productService">Instance of product service.</param>
        /// <param name="logger">Instance of logger.</param>
        public OrderCreatedEventHandler(
            IProductService productService,
            ILogger<OrderCreatedEventHandler> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Handles the OrderCreatedEvent.
        /// </summary>
        /// <param name="orderCreatedEvent">The order created event.</param>
        /// <returns>Task representing the async operation.</returns>
        public async Task HandleAsync(OrderCreatedEvent orderCreatedEvent)
        {
            if (orderCreatedEvent == null)
            {
                _logger.LogError("OrderCreatedEvent is null");
                throw new ArgumentNullException(nameof(orderCreatedEvent));
            }

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["OrderId"] = orderCreatedEvent.OrderId,
                ["CustomerId"] = orderCreatedEvent.CustomerId
            }))
            {
                _logger.LogInformation("Handling OrderCreatedEvent. LineItemsCount: {LineItemsCount}",
                    orderCreatedEvent.LineItems?.Length ?? 0);

                try
                {
                    await _productService.HandleOrderCreatedEvent(orderCreatedEvent);
                    _logger.LogInformation("OrderCreatedEvent handled successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling OrderCreatedEvent");
                    throw;
                }
            }
        }
    }
}
