namespace Retail.UI.Models;

public class MicroserviceConfig
{
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class MonitoringConfig
{
    public int HealthCheckInterval { get; set; } = 10000;
    public int MessageSimulationInterval { get; set; } = 3000;
    public int MaxMessageHistory { get; set; } = 50;
}

public class AppConfig
{
    public Dictionary<string, MicroserviceConfig> Microservices { get; set; } = new();
    public MonitoringConfig Monitoring { get; set; } = new();
}
