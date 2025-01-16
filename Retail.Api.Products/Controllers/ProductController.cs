using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Retail.Api.Products.Common;
using Retail.Api.Products.Dto;
using Retail.Api.Products.Interface;
using Retail.Api.Products.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.Api.Products.Common
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="productService">Intance of product service class.</param>
        public ProductController(IProductService productService)
        {
            _productService = productService;
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
            catch(Exception ex)
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
