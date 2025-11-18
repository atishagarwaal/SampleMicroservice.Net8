using CommonLibrary.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Retail.BFFWeb.Api.Configurations;
using Retail.BFFWeb.Api.Model;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Retail.BFFWeb.Api.Interface
{
    /// <summary>
    /// Order provider class.
    /// </summary>
    public class OrderProvider : IOrderProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OrderServiceConfig _serviceConfig;
        private readonly ILogger<OrderProvider> _logger;
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of HTTP client factory.</param>
        /// <param name="serviceConfig">Instance of order service configuration.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="metrics">Instance of metrics service.</param>
        public OrderProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<OrderServiceConfig> serviceConfig,
            ILogger<OrderProvider> logger,
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
        /// Method to return list of all orders.
        /// </summary>
        /// <returns>List of orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            using (_metrics.TrackDuration("bff_external_call_duration_seconds", "orders", "get_all"))
            {
                this._metrics.IncrementCounter("bff_external_calls_total", 1, "orders", "get_all");
                _logger.LogInformation("Fetching all orders from order service");
                
                try
                {
                    using var client = _httpClientFactory.CreateClient();

                    var url = _serviceConfig.BaseUrl + _serviceConfig.Endpoints.GetAllOrdersV1;
                    _logger.LogDebug("Calling order service endpoint: {Url}", url);
                    
                    var response = await client.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>();
                        var orderCount = data?.Count() ?? 0;
                        this._metrics.IncrementCounter("bff_external_calls_success_total", 1, "orders", "get_all");
                        _logger.LogInformation("Successfully retrieved {OrderCount} orders from order service", orderCount);
                        return data ?? Enumerable.Empty<OrderDto>();
                    }
                    else
                    {
                        this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "orders", "get_all", response.StatusCode.ToString());
                        _logger.LogWarning("Order service returned non-success status code: {StatusCode}", response.StatusCode);
                        return Enumerable.Empty<OrderDto>();
                    }
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "orders", "get_all", "exception");
                    _logger.LogError(ex, "Error fetching all orders from order service");
                    throw;
                }
            }
        }

        /// <summary>
        /// Method to fetch order record based on Id.
        /// </summary>
        /// <param name="id">Order identifier.</param>
        /// <returns>Order object.</returns>
        public async Task<OrderDto> GetOrderByIdAsync(long id)
        {
            using (_metrics.TrackDuration("bff_external_call_duration_seconds", "orders", "get_by_id"))
            {
                this._metrics.IncrementCounter("bff_external_calls_total", 1, "orders", "get_by_id");
                _logger.LogInformation("Fetching order with Id {OrderId} from order service", id);
                
                try
                {
                    using var client = _httpClientFactory.CreateClient();

                    var url = _serviceConfig.BaseUrl + _serviceConfig.Endpoints.GetOrderByIdV1 + "/" + id;
                    _logger.LogDebug("Calling order service endpoint: {Url}", url);

                    var jsonString = await client.GetStringAsync(url);

                    var serviceData = JsonSerializer.Deserialize<OrderDto>(jsonString);

                    if (serviceData == null)
                    {
                        this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "orders", "get_by_id", "null_response");
                        _logger.LogWarning("Order service returned null for OrderId {OrderId}", id);
                    }
                    else
                    {
                        this._metrics.IncrementCounter("bff_external_calls_success_total", 1, "orders", "get_by_id");
                        _logger.LogInformation("Successfully retrieved order with Id {OrderId}", id);
                    }

                    return serviceData;
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "orders", "get_by_id", "exception");
                    _logger.LogError(ex, "Error fetching order with Id {OrderId} from order service", id);
                    throw;
                }
            }
        }
    }
}
