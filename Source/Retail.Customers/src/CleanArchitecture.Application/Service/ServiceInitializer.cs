namespace Retail.Api.Customers.src.CleanArchitecture.Application.Service
{
    using CommonLibrary.Handlers;
    using InventoryUpdatedEventNameSpace;
    using MessagingLibrary.Interface;
    using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;

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
            Console.WriteLine("Customer Service: Initializing subscription to InventoryUpdatedEvent...");
            try
            {
                await _messageSubscriber.SubscribeAsync<InventoryUpdatedEvent>(_inventoryUpdatedHandler.HandleAsync);
                Console.WriteLine("Customer Service: Successfully subscribed to InventoryUpdatedEvent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Customer Service: Error subscribing to InventoryUpdatedEvent - {ex.Message}");
                Console.WriteLine($"Customer Service: Stack trace - {ex.StackTrace}");
                throw;
            }
        }
    }
}
