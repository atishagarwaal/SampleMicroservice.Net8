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
            
            try
            {
                var query = new GetAllOrdersQuery();
                var result = await _mediator.Send(query);
                
                if (result == null)
                {
                    _logger.LogWarning("GetAllOrdersQuery returned null result");
                    return NotFound();
                }
                
                var orderCount = result.Count();
                _logger.LogInformation("Successfully retrieved {OrderCount} orders", orderCount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }

        /// <summary>
        /// Method to fetch order record based on Id.
        /// </summary>
        /// <param name="id">Order identifier.</param>
        /// <returns>Order object.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            _logger.LogInformation("Retrieving order with Id {OrderId}", id);
            
            try
            {
                if (id == 0)
                {
                    _logger.LogWarning("Invalid order Id provided: {OrderId}", id);
                    return BadRequest(MessageConstants.InvalidParameter);
                }
                
                var query = new GetOrderByIdQuery { Id = id };
                var result = await _mediator.Send(query);
                
                if (result == null)
                {
                    _logger.LogWarning("Order with Id {OrderId} not found", id);
                    return NotFound();
                }
                
                _logger.LogInformation("Successfully retrieved order with Id {OrderId}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with Id {OrderId}", id);
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }
    }
}
