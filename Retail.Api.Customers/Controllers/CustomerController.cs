using Microsoft.AspNetCore.Mvc;
using Retail.Api.Customers.Common;
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
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Method to fetch customer record based on Id.
        /// </summary>
        /// <returns>Customer object.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            try
            {
                // Validate parameters
                if (id == 0)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Call business service
                var custObj = this._customerService.GetCustomerById(id);

                // Check if object is null
                if (custObj == null)
                {
                   return NotFound();
                }

                // Return object
                return Ok(custObj);
            }
            catch 
            {
                // Throw exception
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }

        /// <summary>
        /// Method to add a new customer record.
        /// </summary>
        /// <param name="value">Customer name.</param>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        /// <summary>
        /// Method to update a customer record.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        /// <param name="value">Customer name.</param>
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Method to delete a customer record.
        /// </summary>
        /// <param name="id">Customer Id.</param>
        [HttpDelete("{id}")]
        public void Delete(long id)
        {
        }
    }
}
