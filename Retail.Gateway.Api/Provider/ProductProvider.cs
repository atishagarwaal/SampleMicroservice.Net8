using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Retail.BFFWeb.Api.Configurations;
using Retail.BFFWeb.Api.Model;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.BFFWeb.Api.Interface
{
    /// <summary>
    /// Product provider interface.
    /// </summary>
    public class ProductProvider : IProductProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ProductServiceConfig _serviceConfig;

        public ProductProvider(IHttpClientFactory httpClientFactory, IOptions<ProductServiceConfig> serviceConfig)
        {
            if (serviceConfig == null)
            {
                throw new ArgumentNullException(nameof(serviceConfig));
            }

            _httpClientFactory = httpClientFactory;
            _serviceConfig = serviceConfig.Value;
        }

        /// <summary>
        /// Method to return list of all products.
        /// </summary>
        /// <returns>List of products.</returns>
        public async Task<IEnumerable<SkuDto>> GetAllProductsAsync()
        {
            using var client = _httpClientFactory.CreateClient();

            var serviceTask = client.GetStringAsync(_serviceConfig.BaseUrl + _serviceConfig.Endpoints.GetAllProductsV1);
            await serviceTask;

            // Parse JSON responses
            var serviceData = JsonSerializer.Deserialize<IEnumerable<SkuDto>>(serviceTask.Result);

            return serviceData;
        }

        /// <summary>
        /// Method to fetch product record based on Id.
        /// </summary>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> GetProductByIdAsync(long id)
        {
            using var client = _httpClientFactory.CreateClient();

            var serviceTask = client.GetStringAsync(_serviceConfig.BaseUrl + _serviceConfig.Endpoints.GetProductByIdV1 + "/" + id);
            await serviceTask;

            // Parse JSON responses
            var serviceData = JsonSerializer.Deserialize<SkuDto>(serviceTask.Result);

            return serviceData;
        }
    }
}
