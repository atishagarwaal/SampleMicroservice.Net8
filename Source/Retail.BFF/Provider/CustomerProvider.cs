using CommonLibrary.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Retail.BFFWeb.Api.Configurations;
using Retail.BFFWeb.Api.Interface;
using Retail.BFFWeb.Api.Model;
using System.Text.Json;

namespace Retail.BFFWeb.Api.Provider
{
    /// <summary>
    /// Customer provider class.
    /// </summary>
    public class CustomerProvider : ICustomerProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CustomerServiceConfig _serviceConfig;
        private readonly ILogger<CustomerProvider> _logger;
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of HTTP client factory.</param>
        /// <param name="serviceConfig">Instance of customer service configuration.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="metrics">Instance of metrics service.</param>
        public CustomerProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<CustomerServiceConfig> serviceConfig,
            ILogger<CustomerProvider> logger,
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
        /// Method to return list of all customers.
        /// </summary>
        /// <returns>List of customers.</returns>
        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            using (_metrics.TrackDuration("bff_external_call_duration_seconds", "customers", "get_all"))
            {
                _metrics.IncrementCounter("bff_external_calls_total", 1, "customers", "get_all");
                _logger.LogInformation("Fetching all customers from customer service");
                
                try
                {
                    using var client = _httpClientFactory.CreateClient();

                    var url = string.Concat(_serviceConfig.BaseUrl, _serviceConfig.Endpoints.GetAllCustomersV1);
                    _logger.LogDebug("Calling customer service endpoint: {Url}", url);
                    
                    var response = await client.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadFromJsonAsync<IEnumerable<CustomerDto>>();
                        var customerCount = data?.Count() ?? 0;
                        this._metrics.IncrementCounter("bff_external_calls_success_total", 1, "customers", "get_all");
                        _logger.LogInformation("Successfully retrieved {CustomerCount} customers from customer service", customerCount);
                        return data ?? Enumerable.Empty<CustomerDto>();
                    }
                    else
                    {
                        this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "customers", "get_all", response.StatusCode.ToString());
                        _logger.LogWarning("Customer service returned non-success status code: {StatusCode}", response.StatusCode);
                        return Enumerable.Empty<CustomerDto>();
                    }
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "customers", "get_all", "exception");
                    _logger.LogError(ex, "Error fetching all customers from customer service");
                    throw;
                }
            }
        }

        /// <summary>
        /// Method to fetch customer record based on Id.
        /// </summary>
        /// <param name="id">Customer identifier.</param>
        /// <returns>Customer object.</returns>
        public async Task<CustomerDto> GetCustomerByIdAsync(long id)
        {
            using (_metrics.TrackDuration("bff_external_call_duration_seconds", "customers", "get_by_id"))
            {
                this._metrics.IncrementCounter("bff_external_calls_total", 1, "customers", "get_by_id");
                _logger.LogInformation("Fetching customer with Id {CustomerId} from customer service", id);
                
                try
                {
                    using var client = _httpClientFactory.CreateClient();

                    client.BaseAddress = new Uri(_serviceConfig.BaseUrl);

                    var url = _serviceConfig.Endpoints.GetCustomerByIdV1.Replace("{id}", id.ToString());
                    _logger.LogDebug("Calling customer service endpoint: {Url}", url);

                    var jsonString = await client.GetStringAsync(url);

                    var serviceData = JsonSerializer.Deserialize<CustomerDto>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (serviceData == null)
                    {
                        this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "customers", "get_by_id", "null_response");
                        _logger.LogWarning("Customer service returned null for CustomerId {CustomerId}", id);
                    }
                    else
                    {
                        this._metrics.IncrementCounter("bff_external_calls_success_total", 1, "customers", "get_by_id");
                        _logger.LogInformation("Successfully retrieved customer with Id {CustomerId}", id);
                    }

                    return serviceData;
                }
                catch (Exception ex)
                {
                    this._metrics.IncrementCounter("bff_external_calls_errors_total", 1, "customers", "get_by_id", "exception");
                    _logger.LogError(ex, "Error fetching customer with Id {CustomerId} from customer service", id);
                    throw;
                }
            }
        }
    }
}
