using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;

namespace Retail.Api.Products.src.CleanArchitecture.Application.EventHandlers
{
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        public OrderCreatedEventHandler()
        {
        }

        public async Task HandleAsync(OrderCreatedEvent orderCreatedEvent)
        {
            Console.WriteLine($"Order {orderCreatedEvent.OrderId} received in Customer service. Checking Inventory.");
            await Task.CompletedTask;
        }
    }
}
