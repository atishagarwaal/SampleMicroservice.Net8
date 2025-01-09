using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Retail.BFFWeb.Api.Common;
using Retail.BFFWeb.Api.Interface;
using System.Xml;

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
        private readonly IOrderProvider _orderProvider;
        private readonly IProductProvider _productProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BFFController"/> class.
        /// </summary>
        /// <param name="customerProvider">Instance of customer service class.</param>
        /// <param name="orderProvider">Instance of order service class.</param>
        /// <param name="productProvider">Instance of product service class.</param>
        public BFFController(ICustomerProvider customerProvider, IOrderProvider orderProvider, IProductProvider productProvider)
        {
            _customerProvider = customerProvider;
            _orderProvider = orderProvider;
            _productProvider = productProvider;
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
                var customers = await _customerProvider.GetAllCustomersAsync();

                // Call orders API
                var orders = await _orderProvider.GetAllOrdersAsync();

                // Call products API
                var products = await _productProvider.GetAllProductsAsync();

                // Aggregrate data
                var aggregatedData = from o in orders
                                     from c in customers.Where(c => c.Id == o.CustomerId).DefaultIfEmpty()
                                     select new
                {
                    CustomerId = c.Id,
                    CustomerName = string.Concat(c.FirstName, " ", c.LastName),
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                };

                // Return list
                return Ok(aggregatedData);
            }
            catch(Exception ex)
            {
                // Throw exception
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }
       
    }
}
