using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<BFFController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BFFController"/> class.
        /// </summary>
        /// <param name="customerProvider">Instance of customer service class.</param>
        /// <param name="orderProvider">Instance of order service class.</param>
        /// <param name="productProvider">Instance of product service class.</param>
        /// <param name="logger">Instance of logger.</param>
        public BFFController(
            ICustomerProvider customerProvider,
            IOrderProvider orderProvider,
            IProductProvider productProvider,
            ILogger<BFFController> logger)
        {
            _customerProvider = customerProvider;
            _orderProvider = orderProvider;
            _productProvider = productProvider;
            _logger = logger;
        }

        /// <summary>
        /// Method to return list of all order details.
        /// </summary>
        /// <returns>List of order details.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllOrdersDetails()
        {
            _logger.LogInformation("Retrieving all order details with aggregated customer and product data");
            
            try
            {
                var orders = await _orderProvider.GetAllOrdersAsync();
                var orderCount = orders.Count();
                _logger.LogDebug("Retrieved {OrderCount} orders from order service", orderCount);
                
                var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
                var skuIds = orders
                            .SelectMany(o => o.LineItems)
                            .Select(li => li.SkuId)
                            .Distinct()
                            .ToList();

                _logger.LogDebug("Extracted {CustomerCount} unique customer IDs and {SkuCount} unique SKU IDs", 
                    customerIds.Count, skuIds.Count);

                var customerTasks = customerIds.Select(id => _customerProvider.GetCustomerByIdAsync(id));
                var customers = await Task.WhenAll(customerTasks);

                var customerDict = customers.Where(c => c != null).ToDictionary(c => c.Id);
                var validCustomerCount = customerDict.Count;
                if (validCustomerCount < customerIds.Count)
                {
                    _logger.LogWarning("Failed to retrieve {MissingCount} customers out of {TotalCount} requested", 
                        customerIds.Count - validCustomerCount, customerIds.Count);
                }
                _logger.LogDebug("Retrieved {CustomerCount} customers from customer service", validCustomerCount);

                var productTasks = skuIds.Select(id => _productProvider.GetProductByIdAsync(id));
                var products = await Task.WhenAll(productTasks);

                var productDict = products
                    .Where(p => p != null)
                    .ToDictionary(p => p.Id, p => p.Name);
                var validProductCount = productDict.Count;
                if (validProductCount < skuIds.Count)
                {
                    _logger.LogWarning("Failed to retrieve {MissingCount} products out of {TotalCount} requested", 
                        skuIds.Count - validProductCount, skuIds.Count);
                }
                _logger.LogDebug("Retrieved {ProductCount} products from product service", validProductCount);

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
                }).ToList();

                _logger.LogInformation("Successfully aggregated order details. Returning {OrderCount} orders", aggregatedData.Count);
                return Ok(aggregatedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all order details");
                return StatusCode(500, MessageConstants.InternalServerError);
            }
        }
       
    }
}
