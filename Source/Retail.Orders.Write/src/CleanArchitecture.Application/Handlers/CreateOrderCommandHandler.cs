using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary.MessageContract;
using CommonLibrary.Results;
using MediatR;
using MessagingInfrastructure;
using MessagingLibrary.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderCreatedEventNameSpace;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Write.src.CleanArchitecture.Infrastructure.Interfaces;

namespace Retail.Orders.Write.src.CleanArchitecture.Application.Handlers
{
    /// <summary>
    /// Handler for CreateOrderCommand.
    /// </summary>
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConverter<OrderDto, Order> _orderConverter;
        private readonly IConverter<Order, OrderDto> _orderDtoConverter;
        private readonly IMessageValidator<OrderDto> _orderDtoValidator;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOrderCommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of unit of work class.</param>
        /// <param name="orderConverter">Instance of order converter.</param>
        /// <param name="orderDtoConverter">Instance of order DTO converter.</param>
        /// <param name="orderDtoValidator">Instance of order DTO validator.</param>
        /// <param name="messagePublisher">Instance of message publisher.</param>
        /// <param name="serviceScopeFactory">Instance of service scope factory.</param>
        /// <param name="logger">Instance of logger.</param>
        public CreateOrderCommandHandler(
            IUnitOfWork unitOfWork,
            IConverter<OrderDto, Order> orderConverter,
            IConverter<Order, OrderDto> orderDtoConverter,
            IMessageValidator<OrderDto> orderDtoValidator,
            IMessagePublisher messagePublisher,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _orderConverter = orderConverter;
            _orderDtoConverter = orderDtoConverter;
            _orderDtoValidator = orderDtoValidator;
            _messagePublisher = messagePublisher;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Handles the CreateOrderCommand request.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result containing the created order DTO if successful, or an error message if validation fails.</returns>
        public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            if (request?.Order == null)
            {
                this._logger.LogError("CreateOrderCommand or Order is null");
                return Result<OrderDto>.Failure("Order data is required.");
            }

            using (this._logger.BeginScope(new Dictionary<string, object>
            {
                ["CustomerId"] = request.Order.CustomerId,
                ["TotalAmount"] = request.Order.TotalAmount,
                ["LineItemsCount"] = request.Order.LineItems?.Count ?? 0
            }))
            {
                this._logger.LogInformation("Handling CreateOrderCommand. LineItemsCount: {LineItemsCount}",
                    request.Order.LineItems?.Count ?? 0);

                using var scope = this._serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {
                    // Validate order DTO
                    var validationResult = this._orderDtoValidator.Validate(request.Order);
                    if (!validationResult.IsValid)
                    {
                        this._logger.LogWarning("Order validation failed. Validator: {ValidatorName}, Reason: {FailureReason}",
                            validationResult.ValidatorName, validationResult.FailureReason);
                        return Result<OrderDto>.Failure(validationResult.FailureReason ?? "Validation failed");
                    }

                    await unitOfWork.BeginTransactionAsync();

                    // Convert DTO to entity
                    var order = this._orderConverter.Convert(request.Order);

                    this._logger.LogDebug("Converting OrderDto to Order entity. LineItemsCount: {LineItemsCount}",
                        order.LineItems?.Count ?? 0);

                    // Ensure LineItems are properly configured for new entities
                    // EF Core will automatically set OrderId when Order is saved due to navigation property
                    if (order.LineItems != null && order.LineItems.Any())
                    {
                        foreach (var lineItem in order.LineItems)
                        {
                            // Reset Id to 0 for new LineItems (EF will generate it)
                            lineItem.Id = 0;
                            // OrderId will be set automatically by EF Core via navigation property when Order is saved
                            // But we need to ensure the navigation property is set
                            lineItem.Order = order;
                        }
                    }

                    var orderRecord = await unitOfWork.Orders.AddAsync(order);

                    // Save changes to generate the Order ID and save both Order and LineItems
                    // EF Core will automatically set OrderId on LineItems via the navigation property
                    await unitOfWork.CompleteAsync();

                    this._logger.LogDebug("Order saved to database. OrderId: {OrderId}", orderRecord.Id);

                    // Get the complete order with line items
                    var savedOrder = await unitOfWork.Orders.GetByIdAsync(orderRecord.Id);
                    if (savedOrder == null)
                    {
                        this._logger.LogError("Failed to retrieve saved order with ID {OrderId}", orderRecord.Id);
                        await unitOfWork.RollbackTransactionAsync();
                        return Result<OrderDto>.Failure($"Order with ID {orderRecord.Id} was not found after save");
                    }

                    // Create and publish the event
                    var newOrderMessage = new OrderCreatedEvent
                    {
                        CustomerId = savedOrder.CustomerId,
                        OrderDate = savedOrder.OrderDate,
                        TotalAmount = savedOrder.TotalAmount,
                        OrderId = savedOrder.Id,
                        LineItems = savedOrder.LineItems?
                            .Select(item => new OrderCreatedEventNameSpace.LineItem
                            {
                                Id = item.Id,
                                OrderId = savedOrder.Id,
                                SkuId = item.SkuId,
                                Qty = item.Qty,
                            })
                            .ToArray() ?? Array.Empty<OrderCreatedEventNameSpace.LineItem>(),
                    };

                    this._logger.LogInformation("Publishing OrderCreatedEvent. OrderId: {OrderId}, LineItemsCount: {LineItemsCount}",
                        newOrderMessage.OrderId, newOrderMessage.LineItems?.Length ?? 0);
                    await this._messagePublisher.PublishAsync(newOrderMessage, RabbitmqConstants.OrderCreated);
                    this._logger.LogInformation("OrderCreatedEvent published successfully");

                    await unitOfWork.CommitTransactionAsync();

                    this._logger.LogInformation("Order created successfully. OrderId: {OrderId}, CustomerId: {CustomerId}",
                        savedOrder.Id, savedOrder.CustomerId);

                    return Result<OrderDto>.Success(this._orderDtoConverter.Convert(savedOrder));
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Error creating order");
                    await unitOfWork.RollbackTransactionAsync();
                    return Result<OrderDto>.Failure($"An error occurred while creating order: {ex.Message}");
                }
            }
        }
    }
}
