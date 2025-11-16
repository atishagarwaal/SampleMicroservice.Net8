using MessagingLibrary.Interface;
using MessagingLibrary.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Retail.Api.Customers.src.CleanArchitecture.Application.Constants;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Validation.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.Api.Customers.src.CleanArchitecture.API.Controllers
{
    /// <summary>
    /// Customer controller class.
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]

    public class CustomerController : ControllerBase
    {
        private readonly IMessageSubscriber _messageSubscriber;
        private readonly ICustomerService _customerService;
        private readonly IMessageValidator<CustomerDto> _customerDtoValidator;
        private readonly ILogger<CustomerController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        /// <param name="customerService">Instance of customer service class.</param>
        /// <param name="messageSubscriber">Instance of message subscriber class.</param>
        /// <param name="customerDtoValidator">Instance of customer DTO validator.</param>
        /// <param name="logger">Instance of logger.</param>
        public CustomerController(
            ICustomerService customerService,
            IMessageSubscriber messageSubscriber,
            IMessageValidator<CustomerDto> customerDtoValidator,
            ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _messageSubscriber = messageSubscriber;
            _customerDtoValidator = customerDtoValidator;
            _logger = logger;
        }

        /// <summary>
        /// Method to return list of all customers.
        /// </summary>
        /// <returns>List of customers.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("Retrieving all customers");
            
            try
            {
                var list = await _customerService.GetAllCustomersAsync();

                if (list == null)
                {
                    _logger.LogWarning("GetAllCustomersAsync returned null result");
                    return NotFound();
                }

                var customerCount = list.Count();
                _logger.LogInformation("Successfully retrieved {CustomerCount} customers", customerCount);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all customers");
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }

        /// <summary>
        /// Method to fetch customer record based on Id.
        /// </summary>
        /// <param name="id">Customer identifier.</param>
        /// <returns>Customer object.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            _logger.LogInformation("Retrieving customer with Id {CustomerId}", id);
            
            try
            {
                if (id == 0)
                {
                    _logger.LogWarning("Invalid customer Id provided: {CustomerId}", id);
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                var custObj = await _customerService.GetCustomerByIdAsync(id);

                if (custObj == null)
                {
                    _logger.LogWarning("Customer with Id {CustomerId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved customer with Id {CustomerId}", id);
                return Ok(custObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with Id {CustomerId}", id);
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
            if (value == null)
            {
                _logger.LogWarning("Received null customer DTO in POST request");
                return BadRequest(MessageConstants.InvalidParameter);
            }

            _logger.LogInformation("Creating customer. FirstName: {FirstName}, LastName: {LastName}", value.FirstName, value.LastName);
            
            try
            {
                var validationResult = _customerDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Customer validation failed. Validator: {ValidatorName}, Reason: {Reason}",
                        validationResult.ValidatorName, validationResult.FailureReason);
                    return BadRequest(new { error = validationResult.FailureReason, validator = validationResult.ValidatorName });
                }

                var result = await _customerService.AddCustomerAsync(value);

                if (result == null)
                {
                    _logger.LogWarning("AddCustomerAsync returned null result");
                    return StatusCode(500, MessageConstants.InternalServerError);
                }

                _logger.LogInformation("Customer created successfully. CustomerId: {CustomerId}", result.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer. FirstName: {FirstName}, LastName: {LastName}", value.FirstName, value.LastName);
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
            if (id == 0 || value == null)
            {
                _logger.LogWarning("Invalid parameters for update. CustomerId: {CustomerId}, DTO is null: {IsNull}", id, value == null);
                return BadRequest(MessageConstants.InvalidParameter);
            }

            _logger.LogInformation("Updating customer with Id {CustomerId}. FirstName: {FirstName}, LastName: {LastName}", 
                id, value.FirstName, value.LastName);
            
            try
            {
                var validationResult = _customerDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Customer validation failed for update. CustomerId: {CustomerId}, Validator: {ValidatorName}, Reason: {Reason}",
                        id, validationResult.ValidatorName, validationResult.FailureReason);
                    return BadRequest(new { error = validationResult.FailureReason, validator = validationResult.ValidatorName });
                }

                var result = await _customerService.UpdateCustomerAsync(id, value);

                if (result == null)
                {
                    _logger.LogWarning("UpdateCustomerAsync returned null result for CustomerId {CustomerId}", id);
                    return StatusCode(500, MessageConstants.InternalServerError);
                }

                _logger.LogInformation("Customer updated successfully. CustomerId: {CustomerId}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with Id {CustomerId}", id);
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
            _logger.LogInformation("Deleting customer with Id {CustomerId}", id);
            
            try
            {
                if (id == 0)
                {
                    _logger.LogWarning("Invalid customer Id provided for deletion: {CustomerId}", id);
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                var result = await _customerService.DeleteCustomerAsync(id);

                if (result)
                {
                    _logger.LogInformation("Customer deleted successfully. CustomerId: {CustomerId}", id);
                }
                else
                {
                    _logger.LogWarning("Customer with Id {CustomerId} not found for deletion", id);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with Id {CustomerId}", id);
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }
    }
}
