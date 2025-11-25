using System;
using CommonLibrary.Results;
using MediatR;
using MessagingLibrary.Interface;
using MessagingLibrary.Service;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Retail.Orders.Write.src.CleanArchitecture.Application.Commands;
using Retail.Orders.Write.src.CleanArchitecture.Application.Constants;
using Retail.Orders.Write.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Write.src.CleanArchitecture.Application.Interfaces;
using Retail.Orders.Write.src.CleanArchitecture.Application.Validation.Interfaces;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Asp.Versioning;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.Orders.Write.src.CleanArchitecture.API.Controllers
{
    /// <summary>
    /// Customer controller class.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class OrderWriteController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMessageValidator<OrderDto> _orderDtoValidator;
        private readonly ILogger<OrderWriteController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderWriteController"/> class.
        /// </summary>
        /// <param name="mediator">Instance of mediator class.</param>
        /// <param name="orderDtoValidator">Instance of order DTO validator.</param>
        /// <param name="logger">Instance of logger class.</param>
        public OrderWriteController(
            IMediator mediator,
            IMessageValidator<OrderDto> orderDtoValidator,
            ILogger<OrderWriteController> logger)
        {
            _mediator = mediator;
            _orderDtoValidator = orderDtoValidator;
            _logger = logger;
        }

        /// <summary>
        /// Method to add a new order record.
        /// </summary>
        /// <param name="value">Order record.</param>
        /// <returns>Created order DTO.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderDto value)
        {
            if (value == null)
            {
                _logger.LogWarning("Received null order DTO in POST request");
                return Problem(
                    detail: MessageConstants.InvalidParameter,
                    statusCode: 400,
                    title: "Bad Request");
            }

            _logger.LogInformation("Creating order. CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, LineItemsCount: {LineItemsCount}",
                value.CustomerId, value.TotalAmount, value.LineItems?.Count ?? 0);

            try
            {
                // Validate using validator
                var validationResult = _orderDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Order validation failed. Validator: {ValidatorName}, Reason: {FailureReason}",
                        validationResult.ValidatorName, validationResult.FailureReason);
                    return Problem(
                        detail: validationResult.FailureReason,
                        statusCode: 400,
                        title: "Bad Request");
                }

                var command = new CreateOrderCommand { Order = value };
                var result = await this._mediator.Send(command);

                if (result.IsFailure)
                {
                    this._logger.LogWarning("Failed to create order: {Error}", result.Error);
                    return Problem(
                        detail: result.Error,
                        statusCode: 400,
                        title: "Bad Request");
                }

                this._logger.LogInformation("Order created successfully. OrderId: {OrderId}, CustomerId: {CustomerId}",
                    result.Value.Id, result.Value.CustomerId);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error creating order. CustomerId: {CustomerId}, TotalAmount: {TotalAmount}",
                    value.CustomerId, value.TotalAmount);
                return Problem(
                    detail: MessageConstants.InternalServerError,
                    statusCode: 500,
                    title: "Internal Server Error");
            }
        }

        /// <summary>
        /// Method to update an order record.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <param name="value">Order record.</param>
        /// <returns>Updated order DTO.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] OrderDto value)
        {
            if (id == 0 || value == null)
            {
                _logger.LogWarning("Invalid parameters for order update. OrderId: {OrderId}, ValueIsNull: {ValueIsNull}",
                    id, value == null);
                return Problem(
                    detail: MessageConstants.InvalidParameter,
                    statusCode: 400,
                    title: "Bad Request");
            }

            _logger.LogInformation("Updating order. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}",
                id, value.CustomerId, value.TotalAmount);

            try
            {
                // Validate using validator
                var validationResult = _orderDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Order validation failed. OrderId: {OrderId}, Validator: {ValidatorName}, Reason: {FailureReason}",
                        id, validationResult.ValidatorName, validationResult.FailureReason);
                    return Problem(
                        detail: validationResult.FailureReason,
                        statusCode: 400,
                        title: "Bad Request");
                }

                value.Id = id; // Ensure the ID from the route is used
                var command = new UpdateOrderCommand { Order = value };
                var result = await this._mediator.Send(command);

                if (result.IsFailure)
                {
                    this._logger.LogWarning("Failed to update order with Id {OrderId}: {Error}", id, result.Error);
                    
                    // Check if it's a not found error (404) or validation error (400)
                    if (result.Error != null && result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return Problem(
                            detail: result.Error,
                            statusCode: 404,
                            title: "Not Found");
                    }
                    
                    return Problem(
                        detail: result.Error,
                        statusCode: 400,
                        title: "Bad Request");
                }

                this._logger.LogInformation("Order updated successfully. OrderId: {OrderId}, CustomerId: {CustomerId}",
                    id, result.Value.CustomerId);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error updating order. OrderId: {OrderId}, CustomerId: {CustomerId}",
                    id, value.CustomerId);
                return Problem(
                    detail: MessageConstants.InternalServerError,
                    statusCode: 500,
                    title: "Internal Server Error");
            }
        }

        /// <summary>
        /// Method to delete an order record.
        /// </summary>
        /// <param name="id">Order Id.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogInformation("Deleting order. OrderId: {OrderId}", id);

            try
            {
                if (id == 0)
                {
                    _logger.LogWarning("Invalid order ID provided for deletion. OrderId: {OrderId}", id);
                    return Problem(
                        detail: MessageConstants.InvalidParameter,
                        statusCode: 400,
                        title: "Bad Request");
                }

                var command = new DeleteOrderCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result)
                {
                    _logger.LogInformation("Order deleted successfully. OrderId: {OrderId}", id);
                }
                else
                {
                    _logger.LogWarning("Order not found for deletion. OrderId: {OrderId}", id);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order. OrderId: {OrderId}", id);
                return Problem(
                    detail: MessageConstants.InternalServerError,
                    statusCode: 500,
                    title: "Internal Server Error");
            }
        }
    }
}
