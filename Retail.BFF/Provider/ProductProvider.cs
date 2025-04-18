﻿using Microsoft.AspNetCore.Mvc;
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

            var response = await client.GetAsync(_serviceConfig.BaseUrl + _serviceConfig.Endpoints.GetAllProductsV1);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<IEnumerable<SkuDto>>();
                return data ?? Enumerable.Empty<SkuDto>();
            }
            else
            {
                return Enumerable.Empty<SkuDto>();
            }
        }

        /// <summary>
        /// Method to fetch product record based on Id.
        /// </summary>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> GetProductByIdAsync(long id)
        {
            using var client = _httpClientFactory.CreateClient();

            // Set the base address
            client.BaseAddress = new Uri(_serviceConfig.BaseUrl);

            // Construct the request URL
            var url = _serviceConfig.Endpoints.GetProductByIdV1.Replace("{id}", id.ToString());

            var jsonString = await client.GetStringAsync(url);

            // Parse JSON responses
            var serviceData = JsonSerializer.Deserialize<SkuDto>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return serviceData;
        }
    }
}
