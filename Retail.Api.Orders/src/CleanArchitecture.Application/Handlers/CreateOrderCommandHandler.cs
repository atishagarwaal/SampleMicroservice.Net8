using AutoMapper;
using CommonLibrary.MessageContract;
using MediatR;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
using Retail.Api.Orders.src.CleanArchitecture.Application.Dto;
using Retail.Api.Orders.src.CleanArchitecture.Domain.Entities;
using Retail.Api.Orders.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.src.CleanArchitecture.Application.Commands;

namespace Retail.Orders.src.CleanArchitecture.Application.Handlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMessagePublisher _messagePublisher;

        public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IMessagePublisher messagePublisher, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                await unitOfWork.BeginTransactionAsync();
                var order = _mapper.Map<Order>(request.Order);
                var orderRecord = await _unitOfWork.Orders.AddAsync(order);
                await unitOfWork.CompleteAsync();

                var newOrderMessage = new OrderCreatedEvent
                {
                    CustomerId = orderRecord.CustomerId,
                    OrderDate = orderRecord.OrderDate,
                    TotalAmount = orderRecord.TotalAmount,
                    OrderId = orderRecord.Id,
                    LineItems = orderRecord.LineItems.Select(item => new CommonLibrary.Handlers.Dto.LineItemDto
                    {
                        Id = item.Id,
                        OrderId = orderRecord.Id,
                        SkuId = item.SkuId,
                        Qty = item.Qty,
                    }),
                };

                await _messagePublisher.PublishAsync(newOrderMessage, RabbitmqConstants.OrderCreated);
                await unitOfWork.CommitTransactionAsync();

                return _mapper.Map<OrderDto>(orderRecord);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
