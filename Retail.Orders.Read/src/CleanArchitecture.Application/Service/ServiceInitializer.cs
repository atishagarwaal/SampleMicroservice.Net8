﻿using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingLibrary.Interface;
using Retail.Orders.Read.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Orders.Read.src.CleanArchitecture.Application.Service
{
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
            await _messageSubscriber.SubscribeAsync<InventoryUpdatedEvent>(_inventoryUpdatedHandler.HandleAsync);
        }
    }
}
