using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Retail.Api.Products.src.CleanArchitecture.Application.Constants;
using Retail.Api.Products.src.CleanArchitecture.Application.Dto;
using Retail.Api.Products.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Products.src.CleanArchitecture.Application.Validation.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.Api.Products.src.CleanArchitecture.API.Controllers
{
    /// <summary>
    /// Product controller class.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMessageValidator<SkuDto> _skuDtoValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="productService">Instance of product service class.</param>
        /// <param name="skuDtoValidator">Instance of SKU DTO validator.</param>
        public ProductController(IProductService productService, IMessageValidator<SkuDto> skuDtoValidator)
        {
            _productService = productService;
            _skuDtoValidator = skuDtoValidator;
        }

        /// <summary>
        /// Method to return list of all products.
        /// </summary>
        /// <returns>List of products.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Call business service
                var list = await _productService.GetAllProductsAsync();

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
        /// Method to fetch product record based on Id.
        /// </summary>
        /// <returns>Product object.</returns>
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
                var productObj = await _productService.GetProductByIdAsync(id);

                // Check if object is null
                if (productObj == null)
                {
                    return NotFound();
                }

                // Return object
                return Ok(productObj);
            }
            catch (Exception ex)
            {
                // Throw exception
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }

        /// <summary>
        /// Method to add a new product record.
        /// </summary>
        /// <param name="value">Product record.</param>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SkuDto value)
        {
            try
            {
                // Validate parameters
                if (value == null)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Validate using validator
                var validationResult = _skuDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = validationResult.FailureReason, validator = validationResult.ValidatorName });
                }

                // Call business service
                var result = await _productService.AddProductAsync(value);

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
        /// Method to update a product record.
        /// </summary>
        /// <param name="id">Product Id.</param>
        /// <param name="value">Product record.</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] SkuDto value)
        {
            try
            {
                // Validate parameters
                if (id == 0 || value == null)
                {
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Validate using validator
                var validationResult = _skuDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = validationResult.FailureReason, validator = validationResult.ValidatorName });
                }

                // Call business service
                var result = await _productService.UpdateProductAsync(id, value);

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
        /// Method to delete a product record.
        /// </summary>
        /// <param name="id">Product Id.</param>
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
                var result = await _productService.DeleteProductAsync(id);

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
