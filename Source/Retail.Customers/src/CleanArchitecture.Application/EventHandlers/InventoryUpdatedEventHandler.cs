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
            Console.WriteLine($"Customer Service Event Handler: Received InventoryUpdatedEvent - OrderId: {inventoryUpdatedEvent?.OrderId}, CustomerId: {inventoryUpdatedEvent?.CustomerId}");
            try
            {
                await _customerService.HandleOrderCreatedEvent(inventoryUpdatedEvent);
                Console.WriteLine($"Customer Service Event Handler: Successfully processed InventoryUpdatedEvent for OrderId: {inventoryUpdatedEvent?.OrderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Customer Service Event Handler: Error processing InventoryUpdatedEvent - {ex.Message}");
                Console.WriteLine($"Customer Service Event Handler: Stack trace - {ex.StackTrace}");
                throw;
            }
        }
    }
}
