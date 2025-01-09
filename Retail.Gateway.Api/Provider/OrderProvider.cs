using Microsoft.Extensions.Options;
using Retail.BFFWeb.Api.Configurations;
using Retail.BFFWeb.Api.Model;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.BFFWeb.Api.Interface
{
    /// <summary>
    /// Order provider interface.
    /// </summary>
    public class OrderProvider : IOrderProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OrderServiceConfig _serviceConfig;

        public OrderProvider(IHttpClientFactory httpClientFactory, IOptions<OrderServiceConfig> serviceConfig)
        {
            if (serviceConfig == null)
            {
                throw new ArgumentNullException(nameof(serviceConfig));
            }

            _httpClientFactory = httpClientFactory;
            _serviceConfig = serviceConfig.Value;
        }

        /// <summary>
        /// Method to return list of all customers.
        /// </summary>
        /// <returns>List of customers.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            using var client = _httpClientFactory.CreateClient();

            var serviceTask = client.GetStringAsync(_serviceConfig.BaseUrl + _serviceConfig.Endpoints.GetAllOrdersV1);
            await serviceTask;

            // Parse JSON responses
            var serviceData = JsonSerializer.Deserialize<IEnumerable<OrderDto>>(serviceTask.Result);

            return serviceData;
        }

        /// <summary>
        /// Method to fetch order record based on Id.
        /// </summary>
        /// <returns>Order object.</returns>
        public async Task<OrderDto> GetOrderByIdAsync(long id)
        {
            using var client = _httpClientFactory.CreateClient();

            var serviceTask = client.GetStringAsync(_serviceConfig.BaseUrl + _serviceConfig.Endpoints.GetOrderByIdV1 + "/" + id);
            await serviceTask;

            // Parse JSON responses
            var serviceData = JsonSerializer.Deserialize<OrderDto>(serviceTask.Result);

            return serviceData;
        }
    }
}
