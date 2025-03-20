using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingLibrary.Interface;
using Retail.Orders.Read.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Service
{
    internal class ServiceInitializer : IServiceInitializer
    {
        private readonly IMessageSubscriber _messageSubscriber;
        public ServiceInitializer(IMessageSubscriber messageSubscriber)
        {
            _messageSubscriber = messageSubscriber;
        }

        public async Task Initialize()
        {            
        }
    }
}
