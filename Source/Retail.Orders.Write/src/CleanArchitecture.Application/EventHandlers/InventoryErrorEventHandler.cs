using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryErrorEventHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="messagePublisher">Instance of message publisher.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="logger">Instance of logger.</param>
        public InventoryErrorEventHandler(
            IUnitOfWork unitOfWork,
            IMessagePublisher messagePublisher,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<InventoryErrorEventHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
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
                _logger.LogError("InventoryErrorEvent is null");
                throw new ArgumentNullException(nameof(inventoryUpdateFailedEvent));
            }

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["OrderId"] = inventoryUpdateFailedEvent.OrderId,
                ["CustomerId"] = inventoryUpdateFailedEvent.CustomerId
            }))
            {
                _logger.LogWarning("Handling InventoryErrorEvent. OrderId: {OrderId}, CustomerId: {CustomerId}",
                    inventoryUpdateFailedEvent.OrderId, inventoryUpdateFailedEvent.CustomerId);

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var order = await unitOfWork.Orders.GetByIdAsync(inventoryUpdateFailedEvent.OrderId);
                    if (order == null)
                    {
                        _logger.LogWarning("Order not found for deletion. OrderId: {OrderId}", inventoryUpdateFailedEvent.OrderId);
                        throw new Exception("Order does not exist");
                    }

                    _logger.LogInformation("Removing order due to inventory error. OrderId: {OrderId}, CustomerId: {CustomerId}",
                        order.Id, order.CustomerId);

                    await unitOfWork.BeginTransactionAsync();
                    unitOfWork.Orders.Remove(order);
                    await unitOfWork.CompleteAsync();
                    await unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("Order removed successfully due to inventory error. OrderId: {OrderId}",
                        inventoryUpdateFailedEvent.OrderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling InventoryErrorEvent. OrderId: {OrderId}",
                        inventoryUpdateFailedEvent.OrderId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }
    }
}
