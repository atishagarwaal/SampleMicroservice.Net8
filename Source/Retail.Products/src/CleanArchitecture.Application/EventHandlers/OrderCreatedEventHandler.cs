using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using Retail.Api.Products.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Domain.Entities;
using MessagingLibrary.Interface;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text.Json;
using CommonLibrary.Handlers.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;

namespace Retail.Api.Products.src.CleanArchitecture.Application.EventHandlers
{
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly IProductService _productService;
        private readonly IMessagePublisher _messagePublisher;

        public OrderCreatedEventHandler(IProductService productService, IMessagePublisher messagePublisher)
        {
            _productService = productService;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(OrderCreatedEvent orderCreatedEvent)
        {
            _productService.HandleOrderCreatedEvent(orderCreatedEvent);
        }
    }
}
