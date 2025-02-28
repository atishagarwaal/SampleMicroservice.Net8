using CommonLibrary.Handlers;
using MessagingLibrary.Interface;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using CommonLibrary.MessageContract;

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Service
{
    internal class ServiceInitializer : IServiceInitializer
    {
        private readonly IMessageSubscriber _messageSubscriber;
        private readonly IEventHandler<InventoryUpdatedEvent> _inventoryUpdatedHandler;
        public ServiceInitializer(IMessageSubscriber messageSubscriber, IEventHandler<InventoryUpdatedEvent> orderCreatedHandler)
        {
            _messageSubscriber = messageSubscriber;
            _inventoryUpdatedHandler = orderCreatedHandler;
        }

        public async Task Initialize()
        {
            await _messageSubscriber.SubscribeAsync<InventoryUpdatedEvent>(_inventoryUpdatedHandler.HandleAsync);
        }
    }
}
