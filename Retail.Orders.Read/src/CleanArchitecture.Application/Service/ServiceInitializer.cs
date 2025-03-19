using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingLibrary.Interface;
using Retail.Orders.Read.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Service
{
    internal class ServiceInitializer : IServiceInitializer
    {
        private readonly IMessageSubscriber _messageSubscriber;
        private readonly IEventHandler<InventoryErrorEvent> _inventoryErrorHandler;
        public ServiceInitializer(IMessageSubscriber messageSubscriber, IEventHandler<InventoryErrorEvent> inventoryErrorHandler)
        {
            _messageSubscriber = messageSubscriber;
            _inventoryErrorHandler = inventoryErrorHandler;
        }

        public async Task Initialize()
        {
            await _messageSubscriber.SubscribeAsync<InventoryErrorEvent>(_inventoryErrorHandler.HandleAsync);
        }
    }
}
