using CommonLibrary.Handlers;
using MessagingLibrary.Interface;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using CommonLibrary.MessageContract;

namespace Retail.Api.Customers.src.CleanArchitecture.Application.Service
{
    internal class ServiceInitializer : IServiceInitializer
    {
        private readonly IMessageSubscriber _messageSubscriber;
        private readonly IEventHandler<OrderCreatedEvent> _orderCreatedHandler;
        public ServiceInitializer(IMessageSubscriber messageSubscriber, IEventHandler<OrderCreatedEvent> orderCreatedHandler)
        {
            _messageSubscriber = messageSubscriber;
            _orderCreatedHandler = orderCreatedHandler;
        }

        public async Task Initialize()
        {
            await _messageSubscriber.SubscribeAsync<OrderCreatedEvent>(_orderCreatedHandler.HandleAsync);
        }
    }
}
