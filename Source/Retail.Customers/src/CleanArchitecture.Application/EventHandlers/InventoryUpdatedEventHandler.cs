using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingLibrary.Interface;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Api.Customers.src.CleanArchitecture.Application.EventHandlers
{
    public class InventoryUpdatedEventHandler : IEventHandler<InventoryUpdatedEvent>
    {
        private readonly ICustomerService _customerService;
        private readonly IMessagePublisher _messagePublisher;

        public InventoryUpdatedEventHandler(ICustomerService productService, IMessagePublisher messagePublisher)
        {
            _customerService = productService;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(InventoryUpdatedEvent inventoryUpdatedEvent)
        {
            _customerService.HandleOrderCreatedEvent(inventoryUpdatedEvent);
        }
    }
}
