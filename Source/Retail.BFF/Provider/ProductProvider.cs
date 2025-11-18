using CommonLibrary.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Retail.BFFWeb.Api.Configurations;
using Retail.BFFWeb.Api.Model;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.BFFWeb.Api.Interface
{
    /// <summary>
    /// Product provider class.
    /// </summary>
    public class ProductProvider : IProductProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ProductServiceConfig _serviceConfig;
        private readonly ILogger<ProductProvider> _logger;
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of HTTP client factory.</param>
        /// <param name="serviceConfig">Instance of product service configuration.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="metrics">Instance of metrics service.</param>
        public ProductProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<ProductServiceConfig> serviceConfig,
            ILogger<ProductProvider> logger,
            IMetricsService metrics)
        {
            if (serviceConfig == null)
            {
                throw new ArgumentNullException(nameof(serviceConfig));
            }

            _httpClientFactory = httpClientFactory;
            _serviceConfig = serviceConfig.Value;
            _logger = logger;
            _metrics = metrics;
        }

        /// <summary>
        /// Method to return list of all products.
        /// </summary>
        /// <returns>List of products.</returns>
        public async Task<IEnumerable<SkuDto>> GetAllProductsAsync()
        {
            using (_metrics.TrackDuration("bff_external_call_duration_seconds", "products", "get_all"))
            {
                this._metrics.IncrementCounter("bff_external_calls_total", 1, "products", "get_all");
                _logger.LogInformation("Fetching all products from product service");
                
                try
                {
                    using var client = _httpClientFactory.CreateClient();

                    var url = _serviceConfig.BaseUrl + _serviceConfig.Endpoints.GetAllProductsV1;
                    _logger.LogDebug("Calling product service endpoint: {Url}", url);
                    
                    var response = await client.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadFromJsonAsync<IEnumerable<SkuDto>>();
                        var productCount = data?.Count() ?? 0;
                        this._metrics.IncrementCounter("bff_external_calls_success_total", 1, "products", "get_all");
                        _logger.LogInformation("Successfully retrieved {ProductCount} products from product service", productCount);
                        return data ?? Enumerable.Empty<SkuDto>();
                    }
                    else
                    {
                        this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "products", "get_all", response.StatusCode.ToString());
                        _logger.LogWarning("Product service returned non-success status code: {StatusCode}", response.StatusCode);
                        return Enumerable.Empty<SkuDto>();
                    }
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "products", "get_all", "exception");
                    _logger.LogError(ex, "Error fetching all products from product service");
                    throw;
                }
            }
        }

        /// <summary>
        /// Method to fetch product record based on Id.
        /// </summary>
        /// <param name="id">Product identifier.</param>
        /// <returns>Product object.</returns>
        public async Task<SkuDto> GetProductByIdAsync(long id)
        {
            using (_metrics.TrackDuration("bff_external_call_duration_seconds", "products", "get_by_id"))
            {
                this._metrics.IncrementCounter("bff_external_calls_total", 1, "products", "get_by_id");
                _logger.LogInformation("Fetching product with Id {ProductId} from product service", id);
                
                try
                {
                    using var client = _httpClientFactory.CreateClient();

                    client.BaseAddress = new Uri(_serviceConfig.BaseUrl);

                    var url = _serviceConfig.Endpoints.GetProductByIdV1.Replace("{id}", id.ToString());
                    _logger.LogDebug("Calling product service endpoint: {Url}", url);

                    var jsonString = await client.GetStringAsync(url);

                    var serviceData = JsonSerializer.Deserialize<SkuDto>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (serviceData == null)
                    {
                        this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "products", "get_by_id", "null_response");
                        _logger.LogWarning("Product service returned null for ProductId {ProductId}", id);
                    }
                    else
                    {
                        this._metrics.IncrementCounter("bff_external_calls_success_total", 1, "products", "get_by_id");
                        _logger.LogInformation("Successfully retrieved product with Id {ProductId}", id);
                    }

                    return serviceData;
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "products", "get_by_id", "exception");
                    _logger.LogError(ex, "Error fetching product with Id {ProductId} from product service", id);
                    throw;
                }
            }
        }
    }
}
