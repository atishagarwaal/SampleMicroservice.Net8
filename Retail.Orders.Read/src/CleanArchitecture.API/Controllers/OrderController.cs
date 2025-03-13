using MessagingLibrary.Interface;
using MessagingLibrary.Service;
using Microsoft.AspNetCore.Mvc;
using Retail.Api.Orders.Read.src.CleanArchitecture.Application.Constants;
using Retail.Api.Orders.Read.src.CleanArchitecture.Application.Dto;
using Retail.Api.Orders.Read.src.CleanArchitecture.Application.Interfaces;
using System.Runtime.InteropServices;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.Api.Orders.Read.src.CleanArchitecture.API.Controllers
{
    /// <summary>
    /// Customer controller class.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderController"/> class.
        /// </summary>
        /// <param name="orderService">Intance of customer service class.</param>
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Method to return list of all customers.
        /// </summary>
        /// <returns>List of customers.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Call business service
                var list = await _orderService.GetAllOrdersAsync();

                // Check if list is null
                if (list == null)
                {
                    return NotFound();
                }

                // Return list
                return Ok(list);
            }
            catch (Exception ex)
            {
                // Throw exception
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }

        /// <summary>
        /// Method to fetch customer record based on Id.
        /// </summary>
        /// <returns>Customer object.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                // Validate parameters
                if (id == 0)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Call business service
                var orderObj = await _orderService.GetOrderByIdAsync(id);

                // Check if object is null
                if (orderObj == null)
                {
                    return NotFound();
                }

                // Return object
                return Ok(orderObj);
            }
            catch (Exception ex)
            {
                // Throw exception
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }
    }
}
