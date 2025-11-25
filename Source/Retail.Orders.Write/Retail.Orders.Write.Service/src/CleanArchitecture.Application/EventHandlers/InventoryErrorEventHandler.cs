using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLibrary.Exceptions;
using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using CommonLibrary.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MessagingLibrary.Interface;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using InventoryErrorEventNameSpace;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.EventHandlers
{
    /// <summary>
    /// Event handler for InventoryErrorEvent.
    /// </summary>
    public class InventoryErrorEventHandler : IEventHandler<InventoryErrorEvent>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InventoryErrorEventHandler> _logger;
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryErrorEventHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="messagePublisher">Instance of message publisher.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="metrics">Instance of metrics service.</param>
        public InventoryErrorEventHandler(
            IUnitOfWork unitOfWork,
            IMessagePublisher messagePublisher,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<InventoryErrorEventHandler> logger,
            IMetricsService metrics)
        {
            _unitOfWork = unitOfWork;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _metrics = metrics;
        }

        /// <summary>
        /// Handles the InventoryErrorEvent.
        /// </summary>
        /// <param name="inventoryUpdateFailedEvent">The inventory error event.</param>
        /// <returns>Task representing the async operation.</returns>
        public async Task HandleAsync(InventoryErrorEvent inventoryUpdateFailedEvent)
        {
            if (inventoryUpdateFailedEvent == null)
            {
                _metrics.IncrementCounter("message_processing_errors_total", 1, "InventoryErrorEvent", "null_event");
                _logger.LogError("InventoryErrorEvent is null");
                throw new ArgumentNullException(nameof(inventoryUpdateFailedEvent));
            }

            using (_metrics.TrackDuration("message_processing_duration_seconds", "InventoryErrorEvent"))
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["OrderId"] = inventoryUpdateFailedEvent.OrderId,
                ["CustomerId"] = inventoryUpdateFailedEvent.CustomerId
            }))
            {
                this._metrics.IncrementCounter("messages_processed_total", 1, "InventoryErrorEvent");
                this._metrics.IncrementCounter("orders_cancelled_total", 1);
                _logger.LogWarning("Handling InventoryErrorEvent. OrderId: {OrderId}, CustomerId: {CustomerId}",
                    inventoryUpdateFailedEvent.OrderId, inventoryUpdateFailedEvent.CustomerId);

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var order = await unitOfWork.Orders.GetByIdAsync(inventoryUpdateFailedEvent.OrderId);
                    if (order == null)
                    {
                        this._metrics.IncrementCounter("orders_not_found_total", 1);
                        _logger.LogWarning("Order not found for deletion. OrderId: {OrderId}", inventoryUpdateFailedEvent.OrderId);
                        throw new NotFoundException($"Order with ID {inventoryUpdateFailedEvent.OrderId} does not exist");
                    }

                    _logger.LogInformation("Removing order due to inventory error. OrderId: {OrderId}, CustomerId: {CustomerId}",
                        order.Id, order.CustomerId);

                    await unitOfWork.BeginTransactionAsync();
                    unitOfWork.Orders.Remove(order);
                    await unitOfWork.CompleteAsync();
                    await unitOfWork.CommitTransactionAsync();

                    this._metrics.IncrementCounter("orders_cancelled_success_total", 1);
                    _logger.LogInformation("Order removed successfully due to inventory error. OrderId: {OrderId}",
                        inventoryUpdateFailedEvent.OrderId);
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("message_processing_errors_total", 1, "InventoryErrorEvent", "processing_error");
                    _logger.LogError(ex, "Error handling InventoryErrorEvent. OrderId: {OrderId}",
                        inventoryUpdateFailedEvent.OrderId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }
    }
}
