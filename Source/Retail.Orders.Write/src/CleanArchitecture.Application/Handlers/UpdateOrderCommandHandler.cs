using MediatR;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Handlers
{
    /// <summary>
    /// Handler for UpdateOrderCommand.
    /// </summary>
    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConverter<OrderDto, Order> _orderConverter;
        private readonly IConverter<Order, OrderDto> _orderDtoConverter;
        private readonly IMessageValidator<OrderDto> _orderDtoValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateOrderCommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="orderConverter">Instance of order converter.</param>
        /// <param name="orderDtoConverter">Instance of order DTO converter.</param>
        /// <param name="orderDtoValidator">Instance of order DTO validator.</param>
        public UpdateOrderCommandHandler(
            IUnitOfWork unitOfWork,
            IConverter<OrderDto, Order> orderConverter,
            IConverter<Order, OrderDto> orderDtoConverter,
            IMessageValidator<OrderDto> orderDtoValidator)
        {
            _unitOfWork = unitOfWork;
            _orderConverter = orderConverter;
            _orderDtoConverter = orderDtoConverter;
            _orderDtoValidator = orderDtoValidator;
        }

        /// <summary>
        /// Handles the UpdateOrderCommand request.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated order DTO.</returns>
        public async Task<OrderDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate order DTO
                var validationResult = _orderDtoValidator.Validate(request.Order);
                if (!validationResult.IsValid)
                {
                    throw new ArgumentException(validationResult.FailureReason ?? "Validation failed", nameof(request.Order));
                }

                await _unitOfWork.BeginTransactionAsync();
                var order = _orderConverter.Convert(request.Order);
                _unitOfWork.Orders.Update(order);

                if (request.Order.LineItems != null)
                {
                    foreach (var lineItemDto in request.Order.LineItems)
                    {
                        var lineRecord = new LineItem
                        {
                            Id = lineItemDto.Id,
                            OrderId = lineItemDto.OrderId,
                            SkuId = lineItemDto.SkuId,
                            Qty = lineItemDto.Qty,
                        };
                        _unitOfWork.LineItems.Update(lineRecord);
                    }
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                var updatedOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id);
                if (updatedOrder == null)
                {
                    throw new InvalidOperationException($"Order with ID {order.Id} was not found after update");
                }

                return _orderDtoConverter.Convert(updatedOrder);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
