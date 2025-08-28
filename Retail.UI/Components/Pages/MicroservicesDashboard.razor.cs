using Microsoft.AspNetCore.Components;
using Retail.UI.Models;
using System.Text.Json;

namespace Retail.UI.Components.Pages
{
    public partial class MicroservicesDashboard
    {
        // Service status
        public List<ServiceProcessInfo> Services { get; set; } = new();

        // Business data
        public List<ProductDto>? products;
        public List<OrderDto>? orders;
        public List<CustomerDto>? customers;

        // Event tracking
        public List<EventHistory> eventHistory = new();

        // UI state
        public string activeTab = "products";
        public bool showInventoryModal = false;
        public bool showCreateOrderModal = false;
        public ProductDto? selectedProduct;
        public int newInventoryValue;
        public OrderDto newOrder = new();
        public LineItemDto newOrderLineItem = new();

        // Data models
        public class ProductDto
        {
            public long Id { get; set; }
            public string? Name { get; set; }
            public double UnitPrice { get; set; }
            public int Inventory { get; set; }
        }

        public class OrderDto
        {
            public long OrderId { get; set; }
            public long CustomerId { get; set; }
            public string? CustomerName { get; set; }
            public DateTime OrderDate { get; set; }
            public double TotalAmount { get; set; }
            public List<LineItemDto>? LineItems { get; set; }
        }

        public class CustomerDto
        {
            public long Id { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
        }

        public class LineItemDto
        {
            public long Id { get; set; }
            public long OrderId { get; set; }
            public long SkuId { get; set; }
            public int Qty { get; set; }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Logger.LogInformation("Dashboard initialization started");
                await InitializeServices();
                Logger.LogInformation("Services initialized, loading data...");
                await LoadData();
                Logger.LogInformation("Data loaded successfully");
                AddEvent("Dashboard", "Dashboard initialized and ready", "Info");
                Logger.LogInformation("Dashboard initialization completed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during dashboard initialization");
                AddEvent("Dashboard", "Error during initialization: " + ex.Message, "Error");
            }
        }

        private async Task InitializeServices()
        {
            try
            {
                // Initialize with service information
                Services = new List<ServiceProcessInfo>
                {
                    new() { Name = "BFF", Url = "https://localhost:7004", Status = ProcessStatus.Running },
                    new() { Name = "Customers", Url = "https://localhost:7001", Status = ProcessStatus.Running },
                    new() { Name = "Orders.Read", Url = "https://localhost:7005", Status = ProcessStatus.Running },
                    new() { Name = "Orders.Write", Url = "https://localhost:7002", Status = ProcessStatus.Running },
                    new() { Name = "Products", Url = "https://localhost:7003", Status = ProcessStatus.Running }
                };

                // Check actual service status
                await CheckServiceStatus();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error initializing services");
                AddEvent("Dashboard", "Error initializing services: " + ex.Message, "Error");
            }
        }

        private async Task CheckServiceStatus()
        {
            foreach (var service in Services)
            {
                try
                {
                    var response = await Http.GetAsync($"{service.Url}/swagger");
                    var newStatus = response.IsSuccessStatusCode ? ProcessStatus.Running : ProcessStatus.Stopped;
                    
                    if (service.Status != newStatus)
                    {
                        service.Status = newStatus;
                        AddEvent(service.Name, $"Service status changed to {newStatus}", newStatus == ProcessStatus.Running ? "Success" : "Error");
                    }
                }
                catch
                {
                    if (service.Status != ProcessStatus.Stopped)
                    {
                        service.Status = ProcessStatus.Stopped;
                        AddEvent(service.Name, "Service stopped responding", "Error");
                    }
                }
            }
        }

        public async Task LoadData()
        {
            try
            {
                Logger.LogInformation("LoadData method called - starting data refresh");
                await RefreshProducts();
                await RefreshOrders();
                await RefreshCustomers();
                Logger.LogInformation("LoadData completed successfully");
                StateHasChanged(); // Force UI update
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in LoadData method");
                AddEvent("Dashboard", "Error loading data: " + ex.Message, "Error");
            }
        }

        public async Task RefreshProducts()
        {
            try
            {
                Logger.LogInformation("Refreshing products from https://localhost:7003/api/v1.0/Product");
                var response = await Http.GetAsync("https://localhost:7003/api/v1.0/Product");
                Logger.LogInformation("Products API response status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Logger.LogInformation("Products API response content: {Content}", content);
                    
                    products = JsonSerializer.Deserialize<List<ProductDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    Logger.LogInformation("Deserialized {Count} products", products?.Count ?? 0);
                    AddEvent("Products", $"Loaded {products?.Count ?? 0} products", "Success");
                }
                else
                {
                    Logger.LogWarning("Failed to load products. Status: {StatusCode}", response.StatusCode);
                    AddEvent("Products", "Failed to load products", "Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading products");
                AddEvent("Products", "Error loading products: " + ex.Message, "Error");
            }
        }

        public async Task RefreshOrders()
        {
            try
            {
                var response = await Http.GetAsync("https://localhost:7005/api/v1.0/OrderRead");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    orders = JsonSerializer.Deserialize<List<OrderDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    AddEvent("Orders", $"Loaded {orders?.Count ?? 0} orders", "Success");
                }
                else
                {
                    AddEvent("Orders", "Failed to load orders", "Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading orders");
                AddEvent("Orders", "Error loading orders: " + ex.Message, "Error");
            }
        }

        public async Task RefreshCustomers()
    {
        try
        {
                var response = await Http.GetAsync("https://localhost:7001/api/v1.0/Customer");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    customers = JsonSerializer.Deserialize<List<CustomerDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    AddEvent("Customers", $"Loaded {customers?.Count ?? 0} customers", "Success");
                }
                else
                {
                    AddEvent("Customers", "Failed to load customers", "Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading customers");
                AddEvent("Customers", "Error loading customers: " + ex.Message, "Error");
            }
        }

        // Inventory management
        public void ShowInventoryModal(ProductDto product)
        {
            selectedProduct = product;
            newInventoryValue = product.Inventory;
            showInventoryModal = true;
            AddEvent("Inventory", $"Opening inventory adjustment for {product.Name}", "Info");
        }

        public void CloseInventoryModal()
        {
            showInventoryModal = false;
            selectedProduct = null;
        }

        public async Task UpdateInventory()
        {
            if (selectedProduct == null) return;

            try
            {
                AddEvent("Inventory", $"Updating inventory for {selectedProduct.Name} from {selectedProduct.Inventory} to {newInventoryValue}", "Info");

                var updateData = new ProductDto
                {
                    Id = selectedProduct.Id,
                    Name = selectedProduct.Name,
                    UnitPrice = selectedProduct.UnitPrice,
                    Inventory = newInventoryValue
                };

                var response = await Http.PutAsJsonAsync($"https://localhost:7003/api/v1.0/Product/{selectedProduct.Id}", updateData);
            if (response.IsSuccessStatusCode)
            {
                    await RefreshProducts();
                    CloseInventoryModal();
                    AddEvent("Inventory", $"Successfully updated inventory for {selectedProduct.Name} to {newInventoryValue}", "Success");
                }
                else
                {
                    AddEvent("Inventory", $"Failed to update inventory for {selectedProduct.Name}", "Error");
            }
        }
        catch (Exception ex)
        {
                Logger.LogError(ex, "Error updating inventory");
                AddEvent("Inventory", "Error updating inventory: " + ex.Message, "Error");
            }
        }

        // Order management
        public void ShowCreateOrderModal()
        {
            newOrder = new OrderDto
            {
                OrderDate = DateTime.Now,
                TotalAmount = 0
            };
            newOrderLineItem = new LineItemDto();
            showCreateOrderModal = true;
            AddEvent("Orders", "Opening order creation modal", "Info");
        }

        public void CloseCreateOrderModal()
        {
            showCreateOrderModal = false;
            newOrder = new();
            newOrderLineItem = new();
        }

        public async Task CreateOrder()
        {
            try
            {
                // Validate inputs
                if (newOrder.CustomerId <= 0 || newOrderLineItem.SkuId <= 0 || newOrderLineItem.Qty <= 0)
                {
                    AddEvent("Orders", "Invalid order data provided", "Error");
                    return;
                }

                // Get product price to calculate total
                var product = products?.FirstOrDefault(p => p.Id == newOrderLineItem.SkuId);
                if (product == null)
                {
                    AddEvent("Orders", $"Product with ID {newOrderLineItem.SkuId} not found", "Error");
                    return;
                }

                if (product.Inventory < newOrderLineItem.Qty)
                {
                    AddEvent("Orders", $"Insufficient inventory for {product.Name}. Available: {product.Inventory}, Requested: {newOrderLineItem.Qty}", "Error");
                    return;
                }

                AddEvent("Orders", $"Creating order for customer {newOrder.CustomerId} with {newOrderLineItem.Qty} units of {product.Name}", "Info");

                var orderData = new OrderDto
                {
                    CustomerId = newOrder.CustomerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = product.UnitPrice * newOrderLineItem.Qty,
                    LineItems = new List<LineItemDto> { newOrderLineItem }
                };

                var response = await Http.PostAsJsonAsync("https://localhost:7002/api/v1/OrderWrite", orderData);
                if (response.IsSuccessStatusCode)
                {
                    var orderResponse = await response.Content.ReadAsStringAsync();
                    var createdOrder = JsonSerializer.Deserialize<OrderDto>(orderResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    AddEvent("Orders.Write", $"Order created successfully with ID {createdOrder?.OrderId}", "Success");
                    AddEvent("Products", $"Inventory updated for {product.Name} (reduced by {newOrderLineItem.Qty})", "Success");
                    AddEvent("Customers", $"Customer {newOrder.CustomerId} order history updated", "Success");
                    AddEvent("Orders.Read", $"Order {createdOrder?.OrderId} added to read model", "Success");

                    await RefreshOrders();
                    await RefreshProducts(); // Refresh to see inventory changes
                    CloseCreateOrderModal();
                }
                else
                {
                    AddEvent("Orders", "Failed to create order", "Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating order");
                AddEvent("Orders", "Error creating order: " + ex.Message, "Error");
            }
        }

        // Event tracking
        private void AddEvent(string serviceName, string message, string status, string? orderId = null, string? customerId = null, string? productId = null)
        {
            var evt = new EventHistory
            {
                ServiceName = serviceName,
                Message = message,
                Status = status,
                Timestamp = DateTime.Now,
                OrderId = orderId,
                CustomerId = customerId,
                ProductId = productId
            };

            eventHistory.Insert(0, evt); // Add to beginning for newest first

            // Keep only last 50 events
            if (eventHistory.Count > 50)
            {
                eventHistory.RemoveAt(eventHistory.Count - 1);
            }

            StateHasChanged();
        }

        // UI helpers
        public string GetServiceIcon(string serviceName)
        {
            return serviceName switch
            {
                "BFF" => "🏢",
                "Customers" => "👥",
                "Orders.Read" => "📖",
                "Orders.Write" => "📝",
                "Products" => "📦",
                _ => "🔧"
            };
        }

        public void SwitchToTab(string tabName)
        {
            activeTab = tabName;
        }

        public void SwitchToProducts()
        {
            Logger.LogInformation("SwitchToProducts called");
            activeTab = "products";
            StateHasChanged();
        }

        public void SwitchToOrders()
        {
            Logger.LogInformation("SwitchToOrders called");
            activeTab = "orders";
            StateHasChanged();
        }

        public void SwitchToCustomers()
        {
            Logger.LogInformation("SwitchToCustomers called");
            activeTab = "customers";
            StateHasChanged();
        }
    }
}
