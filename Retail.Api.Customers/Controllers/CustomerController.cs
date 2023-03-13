using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Retail.Api.Customers.Common;
using Retail.Api.Customers.Dto;
using Retail.Api.Customers.Interface;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.Api.Customers.Controllers
{
    /// <summary>
    /// Customer controller class.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        /// <param name="customerService">Intance of customer service class.</param>
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
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
                var list = await _customerService.GetAllCustomersAsync();

                // Check if list is null
                if (list == null)
                {
                    return NotFound();
                }

                // Return list
                return Ok(list);
            }
            catch(Exception ex)
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
                var custObj = await _customerService.GetCustomerByIdAsync(id);

                // Check if object is null
                if (custObj == null)
                {
                   return NotFound();
                }

                // Return object
                return Ok(custObj);
            }
            catch (Exception ex)
            {
                // Throw exception
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }

        /// <summary>
        /// Method to add a new customer record.
        /// </summary>
        /// <param name="value">Customer record.</param>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto value)
        {
            try
            {
                // Validate parameters
                if (value == null)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Call business service
                var result = await _customerService.AddCustomerAsync(value);

                // Check if list is null
                if (result == null)
                {
                    return StatusCode(500, MessageConstants.InternalServerError);
                }

                // Return list
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Throw exception
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }

        /// <summary>
        /// Method to update a customer record.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <param name="value">Customer record.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] CustomerDto value)
        {
            try
            {
                // Validate parameters
                if (id == 0 || value == null)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Call business service
                var result = await _customerService.UpdateCustomerAsync(id, value);

                // Check if list is null
                if (result == null)
                {
                    return StatusCode(500, MessageConstants.InternalServerError);
                }

                // Return list
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Throw exception
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
                // Validate parameters
                if (id == 0)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Call business service
                var result = await _customerService.DeleteCustomerAsync(id);

                // Return list
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Throw exception
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }
    }
}
