using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Retail.BFFWeb.Api.Common;
using Retail.BFFWeb.Api.Interface;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.BFFWeb.Api.Controller
{
    /// <summary>
    /// BFF controller class.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BFFController : ControllerBase
    {
        private readonly ICustomerProvider _customerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="productService">Intance of product service class.</param>
        public BFFController(ICustomerProvider customerProvider)
        {
            _customerProvider = customerProvider;
        }

        /// <summary>
        /// Method to return list of all products.
        /// </summary>
        /// <returns>List of products.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllOrdersDetails()
        {
            try
            {
                // Call customer API
                var list = await _customerProvider.GetAllCustomersAsync();

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
       
    }
}
