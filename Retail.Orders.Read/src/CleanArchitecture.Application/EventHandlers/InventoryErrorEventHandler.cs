using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingLibrary.Interface;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text.Json;
using CommonLibrary.Handlers.Dto;
using Retail.Api.Orders.Read.src.CleanArchitecture.Application.Interfaces;

namespace Retail.Api.Products.Read.src.CleanArchitecture.Application.EventHandlers
{
    public class InventoryErrorEventHandler : IEventHandler<InventoryErrorEvent>
    {
        private readonly IOrderService _orderService;
        private readonly IMessagePublisher _messagePublisher;

        public InventoryErrorEventHandler(IOrderService orderService, IMessagePublisher messagePublisher)
        {
            _orderService = orderService;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(InventoryErrorEvent inventoryUpdateFailedEvent)
        {
            //_orderService.HandleInventoryErrorEvent(inventoryUpdateFailedEvent);
        }
    }
}
