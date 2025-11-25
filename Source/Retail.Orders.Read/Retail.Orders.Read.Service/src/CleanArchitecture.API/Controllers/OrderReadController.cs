using MediatR;
using MessagingLibrary.Interface;
using MessagingLibrary.Service;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Retail.Orders.Read.src.CleanArchitecture.Application.Constants;
using Retail.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Orders.Read.src.CleanArchitecture.Application.Interfaces;
using Retail.Orders.Read.src.CleanArchitecture.Application.Queries;
using System.Runtime.InteropServices;
using CommonLibrary.Results;
using Asp.Versioning;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.Orders.Read.src.CleanArchitecture.API.Controllers
{
    /// <summary>
    /// Order read controller class.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class OrderReadController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderReadController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderReadController"/> class.
        /// </summary>
        /// <param name="mediator">Instance of mediator class.</param>
        /// <param name="logger">Instance of logger.</param>
        public OrderReadController(
            IMediator mediator,
            ILogger<OrderReadController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Method to return list of all orders.
        /// </summary>
        /// <returns>List of orders.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("Retrieving all orders");
            
            // Exceptions are handled by GlobalExceptionHandlerMiddleware
            var query = new GetAllOrdersQuery();
            var result = await _mediator.Send(query);
            
            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to retrieve orders: {Error}", result.Error);
                return Problem(
                    detail: result.Error,
                    statusCode: 404,
                    title: "Not Found");
            }
            
            var orderCount = result.Value.Count();
            _logger.LogInformation("Retrieved {OrderCount} orders", orderCount);
            return Ok(result.Value);
        }

        /// <summary>
        /// Method to fetch order record based on Id.
        /// </summary>
        /// <param name="id">Order identifier.</param>
        /// <returns>Order object.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            _logger.LogInformation("Retrieving order. OrderId: {OrderId}", id);
            
            if (id == 0)
            {
                _logger.LogWarning("Invalid order Id provided. OrderId: {OrderId}", id);
                return Problem(
                    detail: MessageConstants.InvalidParameter,
                    statusCode: 400,
                    title: "Bad Request");
            }
            
            // Exceptions are handled by GlobalExceptionHandlerMiddleware
            var query = new GetOrderByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result.IsFailure)
            {
                _logger.LogWarning("Order not found. OrderId: {OrderId}", id);
                return Problem(
                    detail: result.Error,
                    statusCode: 404,
                    title: "Not Found");
            }
            
            return Ok(result.Value);
        }
    }
}
