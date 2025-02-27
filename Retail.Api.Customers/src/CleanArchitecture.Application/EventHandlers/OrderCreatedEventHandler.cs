using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingLibrary.Interface;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Api.Customers.src.CleanArchitecture.Application.EventHandlers
{
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly ICustomerService _customerService;
        private readonly IMessagePublisher _messagePublisher;

        public OrderCreatedEventHandler(ICustomerService productService, IMessagePublisher messagePublisher)
        {
            _customerService = productService;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(OrderCreatedEvent orderCreatedEvent)
        {
        try
        {
            _customerService.HandleOrderCreatedEvent(orderCreatedEvent);

        }
        catch (Exception ex)
        {
        }
    }
    }
}
