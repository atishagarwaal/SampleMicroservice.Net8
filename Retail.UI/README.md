# Retail Microservices Monitoring Dashboard

A real-time monitoring dashboard for the Retail microservices solution built with Blazor Server, featuring **integrated service management** capabilities.

## 🚀 **Key Features**

### **🏥 Service Health Monitoring**
- Real-time health status of all microservices
- Response time monitoring
- Automatic health checks every 10 seconds (configurable)
- Manual health check buttons for each service
- Visual status indicators (Healthy/Unhealthy/Unknown)

### **🎛️ Service Management Console**
- **Start/Stop/Restart** microservices directly from the UI
- **Process Management** - Launch services using `dotnet run`
- **Auto-Detection** of already running services
- **Service Logs** with detailed operation history
- **Bulk Operations** - Start/Stop all services at once

### **🔄 Message Flow Visualization**
- Real-time RabbitMQ message flow between services
- Visual representation of service communication
- Message type tracking (OrderCreated, InventoryUpdated, etc.)
- Error handling and rollback visualization
- Auto-refresh toggle for message simulation

### **📊 Service Status Dashboard**
- Individual service cards with detailed information
- URL endpoints and last check timestamps
- Error message display for failed services
- Responsive grid layout

## 🔧 **Service Launch Behavior**

### **Retail.UI Project**
- **Does NOT automatically launch microservices**
- **Only provides monitoring and control capabilities**
- **Must be started separately from microservices**

### **Microservices**
- **Must be started manually** (either through UI or terminal)
- **Can be launched from the Service Management console**
- **Each service runs in its own process/terminal window**

## 🎯 **How to Use Service Management**

### **1. Launch the Dashboard**
```bash
cd Retail.UI
dotnet run
```

### **2. Access Service Management**
- Navigate to `https://localhost:5001/service-management`
- Or click "Service Management" in the navigation menu

### **3. Control Your Services**
- **Start Service**: Click ▶️ button to launch a microservice
- **Stop Service**: Click ⏹️ button to terminate a service
- **Restart Service**: Click 🔄 button to restart a service
- **Bulk Operations**: Use "Start All Services" or "Stop All Services"

### **4. Monitor Service Status**
- **🟢 Running**: Service is active and responding
- **⚫ Stopped**: Service is not running
- **🟡 Starting**: Service is in the process of starting
- **🟠 Stopping**: Service is in the process of stopping
- **🔴 Error**: Service encountered an error

## 🏗️ **Architecture Overview**

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Retail.UI     │    │  ServiceManager  │    │  Microservices  │
│   (Dashboard)   │◄──►│   (Controller)   │◄──►│  (dotnet run)   │
│                 │    │                  │    │                 │
│ • Monitoring    │    │ • Process Mgmt   │    │ • BFF (7001)    │
│ • Visualization │    │ • Start/Stop     │    │ • Customers     │
│ • Health Check  │    │ • Auto-Detect    │    │   (7002)        │
└─────────────────┘    └──────────────────┘    │ • Orders.Read   │
                                               │   (7003)        │
                                               │ • Orders.Write  │
                                               │   (7004)        │
                                               │ • Products      │
                                               │   (7005)        │
                                               └─────────────────┘
```

## 📋 **Getting Started**

### **Option 1: Manual Service Launch (Traditional)**
1. **Start each microservice manually** in separate terminals:
   ```bash
   # Terminal 1 - BFF
   cd Retail.BFF && dotnet run
   
   # Terminal 2 - Customers  
   cd Retail.Customers && dotnet run
   
   # Terminal 3 - Orders.Read
   cd Retail.Orders.Read && dotnet run
   
   # Terminal 4 - Orders.Write
   cd Retail.Orders.Write && dotnet run
   
   # Terminal 5 - Products
   cd Retail.Products && dotnet run
   ```

2. **Start the UI dashboard**:
   ```bash
   cd Retail.UI && dotnet run
   ```

### **Option 2: UI-Controlled Service Launch (Recommended)**
1. **Start only the UI dashboard**:
   ```bash
   cd Retail.UI && dotnet run
   ```

2. **Use the Service Management console** to start all services:
   - Navigate to `/service-management`
   - Click "🚀 Start All Services"
   - Monitor service startup progress
   - Verify all services are running

## ⚙️ **Configuration**

The dashboard is configured through `appsettings.json`:

```json
{
  "Microservices": {
    "BFF": { "Url": "https://localhost:7001", "Name": "BFF" },
    "Customers": { "Url": "https://localhost:7002", "Name": "Customers" },
    "OrdersRead": { "Url": "https://localhost:7003", "Name": "Orders.Read" },
    "OrdersWrite": { "Url": "https://localhost:7004", "Name": "Orders.Write" },
    "Products": { "Url": "https://localhost:7005", "Name": "Products" }
  },
  "ServiceManagement": {
    "AutoStartOnLaunch": false,
    "ProcessTimeout": 30000,
    "HealthCheckDelay": 5000,
    "LogRetention": 100
  }
}
```

## 🔍 **Service Management Features**

### **Process Control**
- **Automatic Path Detection**: Finds project directories automatically
- **Port Management**: Prevents port conflicts
- **Process Monitoring**: Tracks running processes by PID
- **Graceful Shutdown**: Properly terminates services

### **Service Logs**
- **Operation History**: Start/Stop/Restart events
- **Error Tracking**: Failed operations with error messages
- **Timestamp Logging**: Precise operation timing
- **Log Retention**: Configurable log history (default: 100 entries)

### **Health Verification**
- **Startup Verification**: Confirms services are responding after launch
- **Swagger Endpoint Check**: Verifies service accessibility
- **Response Time Monitoring**: Tracks service performance
- **Error Reporting**: Displays connection issues

## 🚨 **Troubleshooting**

### **Services Won't Start**
1. **Check Project Paths**: Ensure all microservice projects exist
2. **Verify .NET Installation**: Confirm `dotnet` command is available
3. **Port Conflicts**: Check if ports 7001-7005 are already in use
4. **Permissions**: Ensure the UI has permission to launch processes

### **Services Start But Dashboard Shows Unhealthy**
1. **Wait for Startup**: Services need time to fully initialize
2. **Check Swagger Endpoints**: Verify `/swagger` is accessible
3. **Network Issues**: Check firewall and localhost access
4. **Service Configuration**: Verify service URLs in configuration

### **Process Management Issues**
1. **Console Windows**: Services launch in separate console windows
2. **Process Cleanup**: Use the UI to properly stop services
3. **Manual Termination**: If needed, use Task Manager to kill processes
4. **Port Release**: Ensure ports are freed after stopping services

## 🔄 **Workflow Examples**

### **Development Workflow**
1. Start Retail.UI dashboard
2. Use Service Management to start all services
3. Make code changes to a microservice
4. Use "Restart Service" to apply changes
5. Monitor health and message flow

### **Testing Workflow**
1. Start specific services needed for testing
2. Run integration tests
3. Stop services when testing is complete
4. Review service logs for any issues

### **Production Deployment**
1. Start services in dependency order (BFF → Customers → Orders → Products)
2. Monitor service health during startup
3. Verify all services are responding
4. Use health checks to confirm system stability

## 📚 **Dependencies**

- .NET 8.0
- Blazor Server
- System.Text.Json
- Microsoft.Extensions.Configuration
- System.Diagnostics.Process

## 🎉 **Benefits of Integrated Service Management**

✅ **Single Interface**: Manage all services from one dashboard  
✅ **No Terminal Switching**: Start/stop services without multiple terminals  
✅ **Visual Feedback**: Clear status indicators and progress tracking  
✅ **Bulk Operations**: Start or stop all services with one click  
✅ **Process Monitoring**: Track running services and their PIDs  
✅ **Error Handling**: Comprehensive logging and error reporting  
✅ **Health Verification**: Automatic verification of service startup  
✅ **Development Friendly**: Easy restart for code changes  

## 📄 **License**

This project is part of the SampleMicroservice.Net8 solution.
