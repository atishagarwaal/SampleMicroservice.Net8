using CommonLibrary.Handlers;
using Retail.Api.Customers.Interface;
using Retail.Api.Orders.MessageContract;

namespace Retail.Api.Customers.Handlers
{
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        public OrderCreatedEventHandler()
        {
        }

        public async Task HandleAsync(OrderCreatedEvent orderCreatedEvent)
        {
            Console.WriteLine($"Order {orderCreatedEvent.OrderId} received in Customer service. Notifying customer...");
            await Task.CompletedTask;
        }
    }
}
