using CommonLibrary.Handlers;
using CommonLibrary.MessageContract;
using MessagingLibrary.Interface;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text.Json;
using CommonLibrary.Handlers.Dto;
using Retail.Api.Orders.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces;
using AutoMapper;

namespace Retail.Api.Products.src.CleanArchitecture.Application.EventHandlers
{
    public class InventoryErrorEventHandler : IEventHandler<InventoryErrorEvent>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InventoryErrorEventHandler(IUnitOfWork unitOfWork, IMapper mapper, IMessagePublisher messagePublisher, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task HandleAsync(InventoryErrorEvent inventoryUpdateFailedEvent)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var order = await unitOfWork.Orders.GetByIdAsync(inventoryUpdateFailedEvent.OrderId);
                if (order == null)
                {
                    throw new Exception("Order does not exist");
                }

                await unitOfWork.BeginTransactionAsync();
                unitOfWork.Orders.Remove(order);
                await unitOfWork.CompleteAsync();
                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
