using CommonLibrary.Handlers;
using Retail.Api.Customers.Interface;
using Retail.Api.Orders.MessageContract;

namespace Retail.Api.Customers.Handlers
{
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly ICustomerService _customerService;

        public OrderCreatedEventHandler(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task HandleAsync(OrderCreatedEvent orderCreatedEvent)
        {
            Console.WriteLine($"Order {orderCreatedEvent.OrderId} received in Customer service. Notifying customer...");
            await Task.CompletedTask;
        }
    }
}
