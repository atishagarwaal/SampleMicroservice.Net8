namespace Retail.UI.Models;

public class ServiceProcessInfo
{
    public string Name { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Port { get; set; }
    public ProcessStatus Status { get; set; } = ProcessStatus.Stopped;
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public string? ProcessId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsStarting { get; set; } = false;
    public bool IsStopping { get; set; } = false;
}

public enum ProcessStatus
{
    Running,
    Stopped,
    Starting,
    Stopping,
    Error
}
