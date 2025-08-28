namespace Retail.UI.Models;

public class MessageEvent
{
    public string MessageType { get; set; } = string.Empty;
    public string FromService { get; set; } = string.Empty;
    public string ToService { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
}
