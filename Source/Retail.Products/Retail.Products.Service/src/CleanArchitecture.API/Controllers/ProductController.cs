using System;
using Asp.Versioning;
using CommonLibrary.Results;
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
                    return Problem(
                        detail: "No products found",
                        statusCode: 404,
                        title: "Not Found");
                }

                var count = list.Count();
                _logger.LogInformation("Retrieved {ProductCount} products", count);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return Problem(
                    detail: MessageConstants.InternalServerError,
                    statusCode: 500,
                    title: "Internal Server Error");
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
                    return Problem(
                        detail: MessageConstants.InvalidParameter,
                        statusCode: 400,
                        title: "Bad Request");
                }

                // Call business service
                var result = await this._productService.GetProductByIdAsync(id);

                if (result.IsFailure)
                {
                    this._logger.LogWarning("Failed to retrieve product with Id {ProductId}: {Error}", id, result.Error);
                    return Problem(
                        detail: result.Error,
                        statusCode: 404,
                        title: "Not Found");
                }

                this._logger.LogInformation("Product retrieved successfully. ProductId: {ProductId}", id);
                return Ok(result.Value);
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
                return Problem(
                    detail: MessageConstants.InvalidParameter,
                    statusCode: 400,
                    title: "Bad Request");
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
                    return Problem(
                        detail: validationResult.FailureReason,
                        statusCode: 400,
                        title: "Bad Request");
                }

                // Call business service
                var result = await this._productService.AddProductAsync(value);

                if (result.IsFailure)
                {
                    this._logger.LogWarning("Failed to create product: {Error}", result.Error);
                    return Problem(
                        detail: result.Error,
                        statusCode: 400,
                        title: "Bad Request");
                }

                this._logger.LogInformation("Product created successfully. ProductId: {ProductId}, Name: {ProductName}",
                    result.Value.Id, result.Value.Name);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product. Name: {ProductName}", value.Name);
                return Problem(
                    detail: MessageConstants.InternalServerError,
                    statusCode: 500,
                    title: "Internal Server Error");
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
                return Problem(
                    detail: MessageConstants.InvalidParameter,
                    statusCode: 400,
                    title: "Bad Request");
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
                    return Problem(
                        detail: validationResult.FailureReason,
                        statusCode: 400,
                        title: "Bad Request");
                }

                // Call business service
                var result = await this._productService.UpdateProductAsync(id, value);

                if (result.IsFailure)
                {
                    this._logger.LogWarning("Failed to update product with Id {ProductId}: {Error}", id, result.Error);
                    
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

                this._logger.LogInformation("Product updated successfully. ProductId: {ProductId}, Name: {ProductName}",
                    id, result.Value.Name);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product. ProductId: {ProductId}, Name: {ProductName}",
                    id, value.Name);
                return Problem(
                    detail: MessageConstants.InternalServerError,
                    statusCode: 500,
                    title: "Internal Server Error");
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
                    return Problem(
                        detail: MessageConstants.InvalidParameter,
                        statusCode: 400,
                        title: "Bad Request");
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
                return Problem(
                    detail: MessageConstants.InternalServerError,
                    statusCode: 500,
                    title: "Internal Server Error");
            }
        }
    }
}
