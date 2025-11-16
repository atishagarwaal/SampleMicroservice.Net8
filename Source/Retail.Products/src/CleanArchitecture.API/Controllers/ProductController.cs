using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ProductController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="productService">Instance of product service class.</param>
        /// <param name="skuDtoValidator">Instance of SKU DTO validator.</param>
        /// <param name="logger">Instance of logger.</param>
        public ProductController(
            IProductService productService,
            IMessageValidator<SkuDto> skuDtoValidator,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _skuDtoValidator = skuDtoValidator;
            _logger = logger;
        }

        /// <summary>
        /// Method to return list of all products.
        /// </summary>
        /// <returns>List of products.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("Getting all products");
            try
            {
                // Call business service
                var list = await _productService.GetAllProductsAsync();

                // Check if list is null
                if (list == null)
                {
                    _logger.LogWarning("Product list is null");
                    return NotFound();
                }

                var count = list.Count();
                _logger.LogInformation("Retrieved {ProductCount} products", count);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
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
            _logger.LogInformation("Getting product by ID. ProductId: {ProductId}", id);
            try
            {
                // Validate parameters
                if (id == 0)
                {
                    _logger.LogWarning("Invalid product ID provided. ProductId: {ProductId}", id);
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Call business service
                var productObj = await _productService.GetProductByIdAsync(id);

                // Check if object is null
                if (productObj == null)
                {
                    _logger.LogWarning("Product not found. ProductId: {ProductId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Product retrieved successfully. ProductId: {ProductId}", id);
                return Ok(productObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product. ProductId: {ProductId}", id);
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
            if (value == null)
            {
                _logger.LogWarning("Received null product DTO in POST request");
                return BadRequest(MessageConstants.InvalidParameter);
            }

            _logger.LogInformation("Creating product. Name: {ProductName}", value.Name);
            try
            {
                // Validate using validator
                var validationResult = _skuDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Product validation failed. Validator: {ValidatorName}, Reason: {FailureReason}",
                        validationResult.ValidatorName, validationResult.FailureReason);
                    return BadRequest(new { error = validationResult.FailureReason, validator = validationResult.ValidatorName });
                }

                // Call business service
                var result = await _productService.AddProductAsync(value);

                // Check if result is null
                if (result == null)
                {
                    _logger.LogError("Product service returned null result");
                    return StatusCode(500, MessageConstants.InternalServerError);
                }

                _logger.LogInformation("Product created successfully. ProductId: {ProductId}, Name: {ProductName}",
                    result.Id, result.Name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product. Name: {ProductName}", value.Name);
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
            if (id == 0 || value == null)
            {
                _logger.LogWarning("Invalid parameters for product update. ProductId: {ProductId}, ValueIsNull: {ValueIsNull}",
                    id, value == null);
                return BadRequest(MessageConstants.InvalidParameter);
            }

            _logger.LogInformation("Updating product. ProductId: {ProductId}, Name: {ProductName}", id, value.Name);
            try
            {
                // Validate using validator
                var validationResult = _skuDtoValidator.Validate(value);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Product validation failed. ProductId: {ProductId}, Validator: {ValidatorName}, Reason: {FailureReason}",
                        id, validationResult.ValidatorName, validationResult.FailureReason);
                    return BadRequest(new { error = validationResult.FailureReason, validator = validationResult.ValidatorName });
                }

                // Call business service
                var result = await _productService.UpdateProductAsync(id, value);

                // Check if result is null
                if (result == null)
                {
                    _logger.LogError("Product service returned null result. ProductId: {ProductId}", id);
                    return StatusCode(500, MessageConstants.InternalServerError);
                }

                _logger.LogInformation("Product updated successfully. ProductId: {ProductId}, Name: {ProductName}",
                    id, result.Name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product. ProductId: {ProductId}, Name: {ProductName}",
                    id, value.Name);
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
            _logger.LogInformation("Deleting product. ProductId: {ProductId}", id);
            try
            {
                // Validate parameters
                if (id == 0)
                {
                    _logger.LogWarning("Invalid product ID provided for deletion. ProductId: {ProductId}", id);
                    return BadRequest(MessageConstants.InvalidParameter);
                }

                // Call business service
                var result = await _productService.DeleteProductAsync(id);

                if (result)
                {
                    _logger.LogInformation("Product deleted successfully. ProductId: {ProductId}", id);
                }
                else
                {
                    _logger.LogWarning("Product not found for deletion. ProductId: {ProductId}", id);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product. ProductId: {ProductId}", id);
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }
    }
}
