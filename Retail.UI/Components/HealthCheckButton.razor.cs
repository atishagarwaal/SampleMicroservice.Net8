using System.Net.Http;
using Microsoft.AspNetCore.Components;

namespace Retail.UI.Components;

public partial class HealthCheckButton
{
    private bool _isChecking = false;
    private HealthCheckResult? _lastResult;

    [Parameter]
    public string ServiceUrl { get; set; } = string.Empty;

    [Parameter]
    public string ServiceName { get; set; } = string.Empty;

    public bool IsChecking => _isChecking;
    public HealthCheckResult? LastResult => _lastResult;

    private async Task CheckHealth()
    {
        if (string.IsNullOrEmpty(ServiceUrl))
            return;

        _isChecking = true;
        StateHasChanged();

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await Http.GetAsync($"{ServiceUrl}/swagger");
            stopwatch.Stop();

            _lastResult = new HealthCheckResult
            {
                ServiceName = ServiceName,
                IsSuccess = response.IsSuccessStatusCode,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _lastResult = new HealthCheckResult
            {
                ServiceName = ServiceName,
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            _isChecking = false;
            StateHasChanged();
        }
    }
}

public class HealthCheckResult
{
    public string ServiceName { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public long? ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
}
