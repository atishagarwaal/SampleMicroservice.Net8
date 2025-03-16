using AutoMapper;
using MediatR;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Handlers
{
    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var order = _mapper.Map<Order>(request.Order);
                _unitOfWork.Orders.Update(order);

                if (request.Order.LineItems != null)
                {
                    foreach (var lineItem in request.Order.LineItems)
                    {
                        var lineRecord = _mapper.Map<LineItem>(lineItem);
                        _unitOfWork.LineItems.Update(lineRecord);
                    }
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                var updatedOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id);
                return _mapper.Map<OrderDto>(updatedOrder);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
