using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;

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
        private readonly ILogger<UpdateOrderCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateOrderCommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="orderConverter">Instance of order converter.</param>
        /// <param name="orderDtoConverter">Instance of order DTO converter.</param>
        /// <param name="orderDtoValidator">Instance of order DTO validator.</param>
        /// <param name="logger">Instance of logger.</param>
        public UpdateOrderCommandHandler(
            IUnitOfWork unitOfWork,
            IConverter<OrderDto, Order> orderConverter,
            IConverter<Order, OrderDto> orderDtoConverter,
            IMessageValidator<OrderDto> orderDtoValidator,
            ILogger<UpdateOrderCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _orderConverter = orderConverter;
            _orderDtoConverter = orderDtoConverter;
            _orderDtoValidator = orderDtoValidator;
            _logger = logger;
        }

        /// <summary>
        /// Handles the UpdateOrderCommand request.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated order DTO.</returns>
        public async Task<OrderDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            if (request?.Order == null)
            {
                _logger.LogError("UpdateOrderCommand or Order is null");
                throw new ArgumentNullException(nameof(request));
            }

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["OrderId"] = request.Order.Id,
                ["CustomerId"] = request.Order.CustomerId,
                ["LineItemsCount"] = request.Order.LineItems?.Count ?? 0
            }))
            {
                _logger.LogInformation("Handling UpdateOrderCommand. OrderId: {OrderId}, LineItemsCount: {LineItemsCount}",
                    request.Order.Id, request.Order.LineItems?.Count ?? 0);

                try
                {
                    // Validate order DTO
                    var validationResult = _orderDtoValidator.Validate(request.Order);
                    if (!validationResult.IsValid)
                    {
                        _logger.LogWarning("Order validation failed. OrderId: {OrderId}, Validator: {ValidatorName}, Reason: {FailureReason}",
                            request.Order.Id, validationResult.ValidatorName, validationResult.FailureReason);
                        throw new ArgumentException(validationResult.FailureReason ?? "Validation failed", nameof(request.Order));
                    }

                    await _unitOfWork.BeginTransactionAsync();
                    var order = _orderConverter.Convert(request.Order);
                    _unitOfWork.Orders.Update(order);

                    if (request.Order.LineItems != null)
                    {
                        _logger.LogDebug("Updating {LineItemsCount} line items for order", request.Order.LineItems.Count);
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
                        _logger.LogError("Order not found after update. OrderId: {OrderId}", order.Id);
                        throw new InvalidOperationException($"Order with ID {order.Id} was not found after update");
                    }

                    _logger.LogInformation("Order updated successfully. OrderId: {OrderId}, CustomerId: {CustomerId}",
                        updatedOrder.Id, updatedOrder.CustomerId);

                    return _orderDtoConverter.Convert(updatedOrder);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating order. OrderId: {OrderId}", request.Order.Id);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }
    }
}
