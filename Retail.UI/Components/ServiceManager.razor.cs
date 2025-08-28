using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Components;
using Retail.UI.Models;

namespace Retail.UI.Components;

public partial class ServiceManager : IDisposable
{
    private readonly List<ServiceProcessInfo> _services = new();
    private readonly List<ServiceLogEntry> _serviceLogs = new();
    private readonly Dictionary<string, Process?> _processes = new();
    private readonly string _solutionPath;

    public List<ServiceProcessInfo> Services => _services;
    public List<ServiceLogEntry> ServiceLogs => _serviceLogs;
    public bool IsAnyServiceStarting => _services.Any(s => s.IsStarting);
    public bool IsAnyServiceStopping => _services.Any(s => s.IsStopping);

    public ServiceManager()
    {
        // Get the solution directory path
        _solutionPath = Directory.GetCurrentDirectory();
        if (_solutionPath.Contains("Retail.UI"))
        {
            _solutionPath = Directory.GetParent(_solutionPath)?.Parent?.FullName ?? _solutionPath;
        }
    }

    protected override void OnInitialized()
    {
        InitializeServices();
        CheckExistingProcesses();
    }

    private void InitializeServices()
    {
        _services.AddRange(new[]
        {
            new ServiceProcessInfo
            {
                Name = "BFF",
                ProjectPath = Path.Combine(_solutionPath, "Retail.BFF"),
                Url = "https://localhost:7001",
                Port = 7001
            },
            new ServiceProcessInfo
            {
                Name = "Customers",
                ProjectPath = Path.Combine(_solutionPath, "Retail.Customers"),
                Url = "https://localhost:7002",
                Port = 7002
            },
            new ServiceProcessInfo
            {
                Name = "Orders.Read",
                ProjectPath = Path.Combine(_solutionPath, "Retail.Orders.Read"),
                Url = "https://localhost:7003",
                Port = 7003
            },
            new ServiceProcessInfo
            {
                Name = "Orders.Write",
                ProjectPath = Path.Combine(_solutionPath, "Retail.Orders.Write"),
                Url = "https://localhost:7004",
                Port = 7004
            },
            new ServiceProcessInfo
            {
                Name = "Products",
                ProjectPath = Path.Combine(_solutionPath, "Retail.Products"),
                Url = "https://localhost:7005",
                Port = 7005
            }
        });
    }

    private void CheckExistingProcesses()
    {
        foreach (var service in _services)
        {
            var process = GetProcessByPort(service.Port);
            if (process != null)
            {
                service.Status = ProcessStatus.Running;
                service.ProcessId = process.Id.ToString();
                service.StartedAt = DateTime.UtcNow;
                _processes[service.Name] = process;
                AddLog(service.Name, "Detected", $"Found existing process on port {service.Port}", false);
            }
        }
    }

    private Process? GetProcessByPort(int port)
    {
        try
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                try
                {
                    if (process.MainWindowTitle.Contains($"localhost:{port}") || 
                        process.ProcessName.Contains("dotnet"))
                    {
                        return process;
                    }
                }
                catch
                {
                    // Ignore processes we can't access
                }
            }
        }
        catch (Exception ex)
        {
            // Log error without using ILogger to avoid dependency injection issues
            Console.WriteLine($"Error checking processes for port {port}: {ex.Message}");
        }
        return null;
    }

    public async Task StartService(ServiceProcessInfo service)
    {
        if (service.IsStarting || service.Status == ProcessStatus.Running)
            return;

        service.IsStarting = true;
        service.Status = ProcessStatus.Starting;
        service.ErrorMessage = null;
        AddLog(service.Name, "Starting", "Service is starting...", false);

        try
        {
            if (!Directory.Exists(service.ProjectPath))
            {
                throw new DirectoryNotFoundException($"Project path not found: {service.ProjectPath}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run",
                WorkingDirectory = service.ProjectPath,
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal
            };

            var process = new Process { StartInfo = startInfo };
            process.Start();

            _processes[service.Name] = process;
            service.ProcessId = process.Id.ToString();
            service.Status = ProcessStatus.Running;
            service.StartedAt = DateTime.UtcNow;
            service.IsStarting = false;

            AddLog(service.Name, "Started", $"Service started successfully (PID: {process.Id})", false);

            // Wait a bit for the service to fully start
            await Task.Delay(5000);
            await CheckServiceHealth(service);
        }
        catch (Exception ex)
        {
            service.Status = ProcessStatus.Error;
            service.ErrorMessage = ex.Message;
            service.IsStarting = false;
            AddLog(service.Name, "Error", $"Failed to start service: {ex.Message}", true);
            Console.WriteLine($"Failed to start service {service.Name}: {ex.Message}");
        }

        StateHasChanged();
    }

    public async Task StopService(ServiceProcessInfo service)
    {
        if (service.IsStopping || service.Status != ProcessStatus.Running)
            return;

        service.IsStopping = true;
        service.Status = ProcessStatus.Stopping;
        AddLog(service.Name, "Stopping", "Service is stopping...", false);

        try
        {
            if (_processes.TryGetValue(service.Name, out var process) && process != null)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    await process.WaitForExitAsync();
                }
                _processes.Remove(service.Name);
            }

            service.Status = ProcessStatus.Stopped;
            service.ProcessId = null;
            service.StartedAt = null;
            service.IsStopping = false;
            service.StoppedAt = DateTime.UtcNow;

            AddLog(service.Name, "Stopped", "Service stopped successfully", false);
        }
        catch (Exception ex)
        {
            service.Status = ProcessStatus.Error;
            service.ErrorMessage = ex.Message;
            service.IsStopping = false;
            AddLog(service.Name, "Error", $"Failed to stop service: {ex.Message}", true);
            Console.WriteLine($"Failed to stop service {service.Name}: {ex.Message}");
        }

        StateHasChanged();
    }

    public async Task RestartService(ServiceProcessInfo service)
    {
        await StopService(service);
        await Task.Delay(2000); // Wait a bit before restarting
        await StartService(service);
    }

    public async Task StartAllServices()
    {
        var tasks = _services.Where(s => s.Status != ProcessStatus.Running).Select(StartService);
        await Task.WhenAll(tasks);
    }

    public async Task StopAllServices()
    {
        var tasks = _services.Where(s => s.Status == ProcessStatus.Running).Select(StopService);
        await Task.WhenAll(tasks);
    }

    private async Task CheckServiceHealth(ServiceProcessInfo service)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var response = await client.GetAsync($"{service.Url}/swagger");
            
            if (response.IsSuccessStatusCode)
            {
                service.Status = ProcessStatus.Running;
                service.ErrorMessage = null;
            }
            else
            {
                service.Status = ProcessStatus.Error;
                service.ErrorMessage = $"HTTP {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            service.Status = ProcessStatus.Error;
            service.ErrorMessage = ex.Message;
        }
    }

    private void AddLog(string serviceName, string action, string message, bool isError)
    {
        _serviceLogs.Add(new ServiceLogEntry
        {
            ServiceName = serviceName,
            Action = action,
            Message = message,
            IsError = isError,
            Timestamp = DateTime.UtcNow
        });

        // Keep only last 100 logs
        if (_serviceLogs.Count > 100)
        {
            _serviceLogs.RemoveAt(0);
        }
    }

    private string GetStatusIcon(ProcessStatus status) => status switch
    {
        ProcessStatus.Running => "🟢",
        ProcessStatus.Stopped => "⚫",
        ProcessStatus.Starting => "🟡",
        ProcessStatus.Stopping => "🟠",
        ProcessStatus.Error => "🔴",
        _ => "❓"
    };

    public void Dispose()
    {
        foreach (var process in _processes.Values)
        {
            try
            {
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}

public class ServiceLogEntry
{
    public string ServiceName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsError { get; set; }
    public DateTime Timestamp { get; set; }
}
