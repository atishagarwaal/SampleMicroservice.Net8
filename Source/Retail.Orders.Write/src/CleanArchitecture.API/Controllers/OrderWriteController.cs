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
        /// Method to add a new customer record.
        /// </summary>
        /// <param name="value">Customer record.</param>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderDto value)
        {
            try
            {
                if (value == null)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Validate using validator
                var validationResult = _orderDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = validationResult.FailureReason, validator = validationResult.ValidatorName });
                }

                var command = new CreateOrderCommand { Order = value };
                var result = await _mediator.Send(command);
                if (result == null)
                {
                    return StatusCode(500, MessageConstants.InternalServerError);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order. CustomerId: {CustomerId}, TotalAmount: {TotalAmount}", 
                    value?.CustomerId, value?.TotalAmount);
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }

        /// <summary>
        /// Method to update a customer record.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <param name="value">Customer record.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] OrderDto value)
        {
            try
            {
                if (id == 0 || value == null)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Validate using validator
                var validationResult = _orderDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = validationResult.FailureReason, validator = validationResult.ValidatorName });
                }

                value.Id = id; // Ensure the ID from the route is used
                var command = new UpdateOrderCommand { Order = value };
                var result = await _mediator.Send(command);
                if (result == null)
                {
                    return StatusCode(500, MessageConstants.InternalServerError);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order. OrderId: {OrderId}, CustomerId: {CustomerId}", 
                    id, value?.CustomerId);
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }

        /// <summary>
        /// Method to delete a customer record.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }
                var command = new DeleteOrderCommand { Id = id };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order. OrderId: {OrderId}", id);
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }
    }
}
