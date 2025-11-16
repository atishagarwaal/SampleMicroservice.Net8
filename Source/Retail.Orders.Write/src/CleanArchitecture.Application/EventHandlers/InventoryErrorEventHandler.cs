using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingLibrary.Interface;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text.Json;
using Retail.Orders.Write.src.CleanArchitecture.Application.Interfaces;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryErrorEventHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="messagePublisher">Instance of message publisher.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        public InventoryErrorEventHandler(
            IUnitOfWork unitOfWork,
            IMessagePublisher messagePublisher,
            IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Handles the InventoryErrorEvent.
        /// </summary>
        /// <param name="inventoryUpdateFailedEvent">The inventory error event.</param>
        /// <returns>Task representing the async operation.</returns>
        public async Task HandleAsync(InventoryErrorEvent inventoryUpdateFailedEvent)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var order = await unitOfWork.Orders.GetByIdAsync(inventoryUpdateFailedEvent.OrderId);
                if (order == null)
                {
                    throw new Exception("Order does not exist");
                }

                await unitOfWork.BeginTransactionAsync();
                unitOfWork.Orders.Remove(order);
                await unitOfWork.CompleteAsync();
                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
