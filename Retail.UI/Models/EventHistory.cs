namespace Retail.UI.Models
{
    public class EventHistory
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Info"; // Success, Error, Info
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? OrderId { get; set; }
        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
    }
}
