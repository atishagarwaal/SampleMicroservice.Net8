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
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
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
        /// Method to return list of all order details.
        /// </summary>
        /// <returns>List of order details.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllOrdersDetails()
        {
            try
            {
                //  Get orders first (MongoDB)
                var orders = await _orderProvider.GetAllOrdersAsync();
                var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
                var skuIds = orders
                            .SelectMany(o => o.LineItems)
                            .Select(li => li.SkuId)
                            .Distinct()
                            .ToList();

                // Get only required customers (SQL Server)
                var customerTasks = customerIds.Select(id => _customerProvider.GetCustomerByIdAsync(id));
                var customers = await Task.WhenAll(customerTasks);

                // Convert customers list into a dictionary for fast lookups
                var customerDict = customers.Where(c => c != null).ToDictionary(c => c.Id);

                // Get only required products (SQL Server)
                var productTasks = skuIds.Select(id => _productProvider.GetProductByIdAsync(id));
                var products = await Task.WhenAll(productTasks);

                // Convert products to a dictionary for fast lookup
                var productDict = products
                    .Where(p => p != null)
                    .ToDictionary(p => p.Id, p => p.Name);

                // Aggregrate data
                var aggregatedData = orders.Select(o =>
                {
                    customerDict.TryGetValue(o.CustomerId, out var customer);

                    return new
                    {
                        CustomerId = customer?.Id ?? 0,
                        CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown",
                        OrderId = o.Id,
                        OrderDate = o.OrderDate,
                        LineItems = o.LineItems.Select(li => new
                        {
                            SkuId = li.SkuId,
                            SkuName = productDict.TryGetValue(li.SkuId, out var name) ? name : "Unknown",
                            Qty = li.Qty
                        }).ToList()
                    };
                });

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
