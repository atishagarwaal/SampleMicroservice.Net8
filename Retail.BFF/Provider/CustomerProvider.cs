using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Retail.BFFWeb.Api.Configurations;
using Retail.BFFWeb.Api.Interface;
using Retail.BFFWeb.Api.Model;
using System.Text.Json;

namespace Retail.BFFWeb.Api.Provider
{
    public class CustomerProvider : ICustomerProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CustomerServiceConfig _serviceConfig;

        public CustomerProvider(IHttpClientFactory httpClientFactory, IOptions<CustomerServiceConfig> serviceConfig)
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
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            using var client = _httpClientFactory.CreateClient();

            var url = string.Concat(_serviceConfig.BaseUrl, _serviceConfig.Endpoints.GetAllCustomersV1);
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<IEnumerable<CustomerDto>>();
                return data ?? Enumerable.Empty<CustomerDto>();
            }
            else
            {
                return Enumerable.Empty<CustomerDto>();
            }
        }

        /// <summary>
        /// Method to fetch customer record based on Id.
        /// </summary>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> GetCustomerByIdAsync(long id)
        {
            using var client = _httpClientFactory.CreateClient();

            // Set the base address
            client.BaseAddress = new Uri(_serviceConfig.BaseUrl);

            // Construct the request URL
            var url = _serviceConfig.Endpoints.GetCustomerByIdV1.Replace("{id}", id.ToString());

            var jsonString = await client.GetStringAsync(url);

            // Parse JSON responses
            var serviceData = JsonSerializer.Deserialize<CustomerDto>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return serviceData;
        }
    }
}
