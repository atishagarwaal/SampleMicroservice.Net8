namespace Retail.Api.Customers.src.CleanArchitecture.Application.EventHandlers
{
    using CommonLibrary.Handlers;
    using InventoryUpdatedEventNameSpace;
    using MessagingLibrary.Interface;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;

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
