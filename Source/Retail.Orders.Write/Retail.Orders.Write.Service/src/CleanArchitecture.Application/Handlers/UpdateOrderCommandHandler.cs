using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary.Results;
using CommonLibrary.Telemetry;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Result<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConverter<OrderDto, Order> _orderConverter;
        private readonly IConverter<Order, OrderDto> _orderDtoConverter;
        private readonly IMessageValidator<OrderDto> _orderDtoValidator;
        private readonly ILogger<UpdateOrderCommandHandler> _logger;
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateOrderCommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="orderConverter">Instance of order converter.</param>
        /// <param name="orderDtoConverter">Instance of order DTO converter.</param>
        /// <param name="orderDtoValidator">Instance of order DTO validator.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="metrics">Instance of metrics service.</param>
        public UpdateOrderCommandHandler(
            IUnitOfWork unitOfWork,
            IConverter<OrderDto, Order> orderConverter,
            IConverter<Order, OrderDto> orderDtoConverter,
            IMessageValidator<OrderDto> orderDtoValidator,
            ILogger<UpdateOrderCommandHandler> logger,
            IMetricsService metrics)
        {
            _unitOfWork = unitOfWork;
            _orderConverter = orderConverter;
            _orderDtoConverter = orderDtoConverter;
            _orderDtoValidator = orderDtoValidator;
            _logger = logger;
            _metrics = metrics;
        }

        /// <summary>
        /// Handles the UpdateOrderCommand request.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result containing the updated order DTO if successful, or an error message if validation fails or order not found.</returns>
        public async Task<Result<OrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            if (request?.Order == null)
            {
                this._metrics.IncrementCounter("orders_errors_total", 1, "null_request");
                this._logger.LogError("UpdateOrderCommand or Order is null");
                return Result<OrderDto>.Failure("Order data is required.");
            }

            using (this._metrics.TrackDuration("orders_operation_duration_seconds", "update"))
            using (this._logger.BeginScope(new Dictionary<string, object>
            {
                ["OrderId"] = request.Order.Id,
                ["CustomerId"] = request.Order.CustomerId,
                ["LineItemsCount"] = request.Order.LineItems?.Count ?? 0
            }))
            {
                this._metrics.IncrementCounter("orders_updated_attempts_total", 1);
                this._logger.LogInformation("Handling UpdateOrderCommand. OrderId: {OrderId}, LineItemsCount: {LineItemsCount}",
                    request.Order.Id, request.Order.LineItems?.Count ?? 0);

                try
                {
                    // Validate order DTO
                    var validationResult = this._orderDtoValidator.Validate(request.Order);
                    if (!validationResult.IsValid)
                    {
                        this._metrics.IncrementCounter("orders_validation_errors_total", 1);
                        this._logger.LogWarning("Order validation failed. OrderId: {OrderId}, Validator: {ValidatorName}, Reason: {FailureReason}",
                            request.Order.Id, validationResult.ValidatorName, validationResult.FailureReason);
                        return Result<OrderDto>.Failure(validationResult.FailureReason ?? "Validation failed");
                    }

                    // Check if order exists
                    var existingOrder = await this._unitOfWork.Orders.GetByIdAsync(request.Order.Id);
                    if (existingOrder == null)
                    {
                        this._metrics.IncrementCounter("orders_not_found_total", 1);
                        this._logger.LogWarning("Order with Id {OrderId} not found for update", request.Order.Id);
                        return Result<OrderDto>.Failure($"Order with ID {request.Order.Id} not found.");
                    }

                    await this._unitOfWork.BeginTransactionAsync();
                    var order = this._orderConverter.Convert(request.Order);
                    this._unitOfWork.Orders.Update(order);

                    if (request.Order.LineItems != null)
                    {
                        this._logger.LogDebug("Updating {LineItemsCount} line items for order", request.Order.LineItems.Count);
                        foreach (var lineItemDto in request.Order.LineItems)
                        {
                            var lineRecord = new LineItem
                            {
                                Id = lineItemDto.Id,
                                OrderId = lineItemDto.OrderId,
                                SkuId = lineItemDto.SkuId,
                                Qty = lineItemDto.Qty,
                            };
                            this._unitOfWork.LineItems.Update(lineRecord);
                        }
                    }

                    await this._unitOfWork.CompleteAsync();
                    await this._unitOfWork.CommitTransactionAsync();

                    var updatedOrder = await this._unitOfWork.Orders.GetByIdAsync(order.Id);
                    if (updatedOrder == null)
                    {
                        // This is unexpected - order should exist after update
                        this._logger.LogError("Order not found after update. OrderId: {OrderId}", order.Id);
                        throw new InvalidOperationException($"Order with ID {order.Id} was not found after update");
                    }

                    this._metrics.IncrementCounter("orders_updated_total", 1);
                    this._logger.LogInformation("Order updated successfully. OrderId: {OrderId}, CustomerId: {CustomerId}",
                        updatedOrder.Id, updatedOrder.CustomerId);

                    return Result<OrderDto>.Success(this._orderDtoConverter.Convert(updatedOrder));
                }
                catch (DbUpdateException ex)
                {
                    // Unexpected error: database failure
                    this._metrics.IncrementCounter("orders_errors_total", 1, "update");
                    this._logger.LogError(ex, "Database error updating order. OrderId: {OrderId}", request.Order.Id);
                    await this._unitOfWork.RollbackTransactionAsync();
                    throw; // Let middleware handle
                }
                catch (Exception ex)
                {
                    // Unexpected error: system failure
                    this._metrics.IncrementCounter("orders_errors_total", 1, "update");
                    this._logger.LogError(ex, "Unexpected error updating order. OrderId: {OrderId}", request.Order.Id);
                    await this._unitOfWork.RollbackTransactionAsync();
                    throw; // Let middleware handle
                }
            }
        }
    }
}
