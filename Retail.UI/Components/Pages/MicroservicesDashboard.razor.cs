using System.Timers;
using System.Text.Json;
using Retail.UI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components;

namespace Retail.UI.Components.Pages;

public partial class MicroservicesDashboard : IDisposable
{
    private System.Timers.Timer? _monitoringTimer;
    private readonly List<MicroserviceStatus> _microservices = new();
    private readonly List<MessageEvent> _recentMessages = new();
    private bool _isConnected = false;

    [Inject]
    public IConfiguration Configuration { get; set; } = default!;

    public List<MicroserviceStatus> Microservices => _microservices;
    public List<MessageEvent> RecentMessages => _recentMessages;
    public bool IsConnected => _isConnected;

    protected override void OnInitialized()
    {
        InitializeMicroservices();
        StartMonitoring();
        StateHasChanged();
    }

    private void InitializeMicroservices()
    {
        var config = new AppConfig();
        Configuration.GetSection("Microservices").Bind(config.Microservices);
        Configuration.GetSection("Monitoring").Bind(config.Monitoring);

        // Initialize microservices from configuration
        foreach (var (key, serviceConfig) in config.Microservices)
        {
            _microservices.Add(new MicroserviceStatus
            {
                Name = serviceConfig.Name,
                Url = serviceConfig.Url,
                Status = ServiceStatus.Unknown
            });
        }

        // Add some sample message events for demonstration
        _recentMessages.AddRange(new[]
        {
            new MessageEvent
            {
                MessageType = "OrderCreated",
                FromService = "BFF",
                ToService = "Orders.Write",
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                IsError = false
            },
            new MessageEvent
            {
                MessageType = "InventoryUpdateFailed",
                FromService = "Orders.Write",
                ToService = "Products",
                Timestamp = DateTime.UtcNow.AddMinutes(-3),
                IsError = true,
                ErrorMessage = "Insufficient inventory"
            },
            new MessageEvent
            {
                MessageType = "OrderCreated",
                FromService = "Orders.Write",
                ToService = "Orders.Read",
                Timestamp = DateTime.UtcNow.AddMinutes(-2),
                IsError = false
            }
        });
    }

    private void StartMonitoring()
    {
        var interval = Configuration.GetValue<int>("Monitoring:HealthCheckInterval", 10000);
        _monitoringTimer = new System.Timers.Timer(interval);
        _monitoringTimer.Elapsed += async (sender, e) => await CheckServicesHealth();
        _monitoringTimer.AutoReset = true;
        _monitoringTimer.Start();

        // Initial check
        _ = Task.Run(async () => await CheckServicesHealth());
    }

    private async Task CheckServicesHealth()
    {
        var tasks = _microservices.Select(CheckServiceHealth);
        await Task.WhenAll(tasks);
        
        await InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }

    private async Task CheckServiceHealth(MicroserviceStatus service)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Try to access the health endpoint or swagger
            var response = await Http.GetAsync($"{service.Url}/swagger");
            stopwatch.Stop();
            
            service.ResponseTime = stopwatch.ElapsedMilliseconds;
            service.LastChecked = DateTime.UtcNow;
            service.Status = response.IsSuccessStatusCode ? ServiceStatus.Healthy : ServiceStatus.Unhealthy;
            service.ErrorMessage = null;
            
            if (response.IsSuccessStatusCode)
            {
                _isConnected = true;
            }
        }
        catch (Exception ex)
        {
            service.Status = ServiceStatus.Unhealthy;
            service.ErrorMessage = ex.Message;
            service.LastChecked = DateTime.UtcNow;
            service.ResponseTime = null;
            _isConnected = false;
        }
    }

    public void Dispose()
    {
        _monitoringTimer?.Stop();
        _monitoringTimer?.Dispose();
    }
}

public class MicroserviceStatus
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; }
    public DateTime? LastChecked { get; set; }
    public long? ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum ServiceStatus
{
    Healthy,
    Unhealthy,
    Unknown
}
