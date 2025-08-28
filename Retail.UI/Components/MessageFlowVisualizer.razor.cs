using System.Timers;
using Microsoft.AspNetCore.Components;
using Retail.UI.Models;

namespace Retail.UI.Components;

public partial class MessageFlowVisualizer : IDisposable
{
    private System.Timers.Timer? _messageTimer;
    private readonly List<MessageEvent> _messages = new();
    private bool _isAutoRefresh = true;

    [Parameter]
    public List<MessageEvent> Messages { get; set; } = new();

    public bool IsAutoRefresh => _isAutoRefresh;

    protected override void OnInitialized()
    {
        if (_isAutoRefresh)
        {
            StartMessageSimulation();
        }
    }

    private void StartMessageSimulation()
    {
        _messageTimer = new System.Timers.Timer(3000); // Simulate messages every 3 seconds
        _messageTimer.Elapsed += (sender, e) => SimulateMessage();
        _messageTimer.AutoReset = true;
        _messageTimer.Start();
    }

    private void SimulateMessage()
    {
        var random = new Random();
        var messageTypes = new[] { "OrderCreated", "InventoryUpdated", "CustomerUpdated", "ProductUpdated" };
        var services = new[] { "BFF", "Customers", "Orders.Write", "Orders.Read", "Products" };
        
        var message = new MessageEvent
        {
            MessageType = messageTypes[random.Next(messageTypes.Length)],
            FromService = services[random.Next(services.Length)],
            ToService = services[random.Next(services.Length)],
            Timestamp = DateTime.UtcNow,
            IsError = random.Next(10) == 0, // 10% chance of error
            ErrorMessage = random.Next(10) == 0 ? "Connection timeout" : null
        };

        _messages.Add(message);
        
        // Keep only last 50 messages
        if (_messages.Count > 50)
        {
            _messages.RemoveAt(0);
        }

        InvokeAsync(StateHasChanged);
    }

    private int GetMessageCount(string serviceName)
    {
        return _messages.Count(m => m.FromService == serviceName || m.ToService == serviceName);
    }

    private void ToggleAutoRefresh()
    {
        _isAutoRefresh = !_isAutoRefresh;
        
        if (_isAutoRefresh)
        {
            StartMessageSimulation();
        }
        else
        {
            _messageTimer?.Stop();
        }
        
        StateHasChanged();
    }

    private void ClearMessages()
    {
        _messages.Clear();
        StateHasChanged();
    }

    public void Dispose()
    {
        _messageTimer?.Stop();
        _messageTimer?.Dispose();
    }
}
