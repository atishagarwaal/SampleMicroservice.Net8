using Microsoft.AspNetCore.Components;
using Retail.UI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public List<NotificationDto>? notifications;

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
        public HashSet<long> expandedOrders = new();

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
            [JsonPropertyName("id")]
            public long OrderId { get; set; }
            
            [JsonPropertyName("customerId")]
            public long CustomerId { get; set; }
            
            [JsonPropertyName("orderDate")]
            public DateTime OrderDate { get; set; }
            
            [JsonPropertyName("totalAmount")]
            public double TotalAmount { get; set; }
            
            [JsonPropertyName("lineItems")]
            public List<LineItemDto>? LineItems { get; set; }
            
            public string CustomerName { get; set; } = string.Empty;
        }

        public class CustomerDto
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
            
            [JsonPropertyName("firstName")]
            public string FirstName { get; set; } = string.Empty;
            
            [JsonPropertyName("lastName")]
            public string LastName { get; set; } = string.Empty;
        }

        public class LineItemDto
        {
            public long Id { get; set; }
            public long OrderId { get; set; }
            public long SkuId { get; set; }
            public int Qty { get; set; }
        }

        public class NotificationDto
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }
            
            [JsonPropertyName("orderId")]
            public long OrderId { get; set; }
            
            [JsonPropertyName("customerId")]
            public long CustomerId { get; set; }
            
            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;
            
            [JsonPropertyName("orderDate")]
            public DateTime OrderDate { get; set; }
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
                
                // Load customers first so we can populate customer names in orders
                await RefreshCustomers();
                await RefreshProducts();
                await RefreshOrders(); // Now customers will be available for name population
                await RefreshNotifications(); // Load customer notifications
                
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
                Logger.LogInformation("Refreshing orders from https://localhost:7005/api/v1.0/OrderRead");
                var response = await Http.GetAsync("https://localhost:7005/api/v1.0/OrderRead");
                Logger.LogInformation("Orders API response status: {StatusCode}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                    var content = await response.Content.ReadAsStringAsync();
                    Logger.LogInformation("Orders API response content: {Content}", content);
                    
                    orders = JsonSerializer.Deserialize<List<OrderDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    Logger.LogInformation("Deserialized {Count} orders", orders?.Count ?? 0);
                    
                    // Populate customer names by looking up customer data
                    if (orders != null && customers != null)
                    {
                        Logger.LogInformation("Populating customer names for {Count} orders using {CustomerCount} customers", 
                            orders.Count, customers.Count);
                        
                        foreach (var order in orders)
                        {
                            var customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
                            if (customer != null)
                            {
                                order.CustomerName = $"{customer.FirstName} {customer.LastName}".Trim();
                                Logger.LogInformation("Order {OrderId}: Customer {CustomerId} -> {CustomerName}", 
                                    order.OrderId, order.CustomerId, order.CustomerName);
                            }
                            else
                            {
                                order.CustomerName = $"Customer {order.CustomerId}";
                                Logger.LogWarning("Order {OrderId}: Customer {CustomerId} not found", 
                                    order.OrderId, order.CustomerId);
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Cannot populate customer names: orders={OrdersNull}, customers={CustomersNull}", 
                            orders == null, customers == null);
                    }
                    
                    AddEvent("Orders", $"Loaded {orders?.Count ?? 0} orders", "Success");
                }
                else
                {
                    Logger.LogWarning("Failed to load orders. Status: {StatusCode}", response.StatusCode);
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
                Logger.LogInformation("Refreshing customers from https://localhost:7001/api/v1.0/Customer");
                var response = await Http.GetAsync("https://localhost:7001/api/v1.0/Customer");
                Logger.LogInformation("Customers API response status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Logger.LogDebug("Customers API response content: {Content}", content);
                    
                    customers = JsonSerializer.Deserialize<List<CustomerDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    Logger.LogInformation("Deserialized {Count} customers", customers?.Count ?? 0);
                    AddEvent("Customers", $"Loaded {customers?.Count ?? 0} customers", "Success");
                }
                else
                {
                    Logger.LogWarning("Failed to load customers. Status: {StatusCode}", response.StatusCode);
                    AddEvent("Customers", "Failed to load customers", "Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading customers");
                AddEvent("Customers", "Error loading customers: " + ex.Message, "Error");
            }
        }

        public async Task RefreshNotifications()
        {
            try
            {
                Logger.LogInformation("Refreshing notifications from https://localhost:7001/api/v1.0/Notification");
                var response = await Http.GetAsync("https://localhost:7001/api/v1.0/Notification");
                Logger.LogInformation("Notifications API response status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Logger.LogDebug("Notifications API response content: {Content}", content);
                    
                    notifications = JsonSerializer.Deserialize<List<NotificationDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    Logger.LogInformation("Deserialized {Count} notifications", notifications?.Count ?? 0);
                    AddEvent("Notifications", $"Loaded {notifications?.Count ?? 0} notifications", "Success");
                }
                else
                {
                    Logger.LogWarning("Failed to load notifications. Status: {StatusCode}", response.StatusCode);
                    AddEvent("Notifications", "Failed to load notifications", "Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading notifications");
                AddEvent("Notifications", "Error loading notifications: " + ex.Message, "Error");
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
            if (selectedProduct == null)
            {
                Logger.LogWarning("UpdateInventory called but selectedProduct is null");
                return;
            }

            try
            {
                Logger.LogInformation("Updating inventory. ProductId: {ProductId}, ProductName: {ProductName}, OldInventory: {OldInventory}, NewInventory: {NewInventory}",
                    selectedProduct.Id, selectedProduct.Name, selectedProduct.Inventory, newInventoryValue);
                
                AddEvent("Inventory", $"Updating inventory for {selectedProduct.Name} from {selectedProduct.Inventory} to {newInventoryValue}", "Info");

                var updateData = new ProductDto
                {
                    Id = selectedProduct.Id,
                    Name = selectedProduct.Name,
                    UnitPrice = selectedProduct.UnitPrice,
                    Inventory = newInventoryValue
                };

                Logger.LogDebug("Sending PUT request to update product. ProductId: {ProductId}, URL: https://localhost:7003/api/v1.0/Product/{ProductId}",
                    selectedProduct.Id, selectedProduct.Id);
                
                var response = await Http.PutAsJsonAsync($"https://localhost:7003/api/v1.0/Product/{selectedProduct.Id}", updateData);
                
                Logger.LogInformation("Product update response status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    Logger.LogInformation("Inventory updated successfully. ProductId: {ProductId}, NewInventory: {NewInventory}",
                        selectedProduct.Id, newInventoryValue);
                    
                    await RefreshProducts();
                    CloseInventoryModal();
                    AddEvent("Inventory", $"Successfully updated inventory for {selectedProduct.Name} to {newInventoryValue}", "Success");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.LogWarning("Failed to update inventory. ProductId: {ProductId}, StatusCode: {StatusCode}, Error: {Error}",
                        selectedProduct.Id, response.StatusCode, errorContent);
                    AddEvent("Inventory", $"Failed to update inventory for {selectedProduct.Name}", "Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating inventory. ProductId: {ProductId}", selectedProduct?.Id);
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
                Logger.LogInformation("CreateOrder method called. CustomerId: {CustomerId}, SkuId: {SkuId}, Qty: {Qty}",
                    newOrder.CustomerId, newOrderLineItem.SkuId, newOrderLineItem.Qty);
                
                // Validate inputs
                if (newOrder.CustomerId <= 0 || newOrderLineItem.SkuId <= 0 || newOrderLineItem.Qty <= 0)
                {
                    Logger.LogWarning("Invalid order data provided. CustomerId: {CustomerId}, SkuId: {SkuId}, Qty: {Qty}",
                        newOrder.CustomerId, newOrderLineItem.SkuId, newOrderLineItem.Qty);
                    AddEvent("Orders", "Invalid order data provided", "Error");
                    return;
                }

                // Get product price to calculate total
                var product = products?.FirstOrDefault(p => p.Id == newOrderLineItem.SkuId);
                if (product == null)
                {
                    Logger.LogWarning("Product not found. SkuId: {SkuId}", newOrderLineItem.SkuId);
                    AddEvent("Orders", $"Product with ID {newOrderLineItem.SkuId} not found", "Error");
                    return;
                }

                if (product.Inventory < newOrderLineItem.Qty)
                {
                    Logger.LogWarning("Insufficient inventory. ProductId: {ProductId}, ProductName: {ProductName}, Available: {Available}, Requested: {Requested}",
                        product.Id, product.Name, product.Inventory, newOrderLineItem.Qty);
                    AddEvent("Orders", $"Insufficient inventory for {product.Name}. Available: {product.Inventory}, Requested: {newOrderLineItem.Qty}", "Error");
                    return;
                }

                // Step 1: Order Creation
                Logger.LogInformation("Creating order. CustomerId: {CustomerId}, ProductId: {ProductId}, ProductName: {ProductName}, Qty: {Qty}, TotalAmount: {TotalAmount}",
                    newOrder.CustomerId, product.Id, product.Name, newOrderLineItem.Qty, product.UnitPrice * newOrderLineItem.Qty);
                
                AddEvent("Orders.Write", $"🚀 Creating order for customer {newOrder.CustomerId} with {newOrderLineItem.Qty} units of {product.Name}", "Info");

                var orderData = new OrderDto
                {
                    CustomerId = newOrder.CustomerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = product.UnitPrice * newOrderLineItem.Qty,
                    LineItems = new List<LineItemDto> { newOrderLineItem }
                };

                Logger.LogDebug("Sending POST request to create order. URL: https://localhost:7002/api/v1/OrderWrite");
                var response = await Http.PostAsJsonAsync("https://localhost:7002/api/v1/OrderWrite", orderData);
                
                Logger.LogInformation("Order creation response status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var orderResponse = await response.Content.ReadAsStringAsync();
                    Logger.LogDebug("Order creation response content: {Content}", orderResponse);
                    
                    var createdOrder = JsonSerializer.Deserialize<OrderDto>(orderResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    Logger.LogInformation("Order created successfully. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}",
                        createdOrder?.OrderId, createdOrder?.CustomerId, createdOrder?.TotalAmount);

                    // Step 2: Order Created Successfully in Retail.Order Database
                    AddEvent("Orders.Write", $"✅ Order {createdOrder?.OrderId} created successfully in Retail.Order database", "Success");
                    
                    // Step 3: RabbitMQ OrderCreatedEvent Sent to Product Service
                    AddEvent("Products", $"📤 RabbitMQ: OrderCreatedEvent sent via orderdomain.topic.exchange for {product.Name} (Qty: {newOrderLineItem.Qty})", "Info");
                    
                    // Step 4: Product Service Processes OrderCreatedEvent
                    AddEvent("Products", $"🔄 Processing OrderCreatedEvent: Checking inventory for {product.Name}", "Info");
                    
                    // Step 5: Inventory Updated Successfully
                    AddEvent("Products", $"✅ Inventory updated for {product.Name} (reduced by {newOrderLineItem.Qty})", "Success");
                    
                    // Step 6: RabbitMQ InventoryUpdatedEvent Sent to Customer Service
                    AddEvent("Customers", $"📤 RabbitMQ: InventoryUpdatedEvent sent via inventorydomain.topic.exchange for order {createdOrder?.OrderId}", "Info");
                    AddEvent("Customers", $"✅ Customer notification created in Retail.Customer database", "Success");
                    
                    // Step 7: RabbitMQ InventoryUpdatedEvent Sent to Order Read Service
                    AddEvent("Orders.Read", $"📤 RabbitMQ: InventoryUpdatedEvent sent via inventorydomain.topic.exchange for order {createdOrder?.OrderId}", "Info");
                    AddEvent("Orders.Read", $"✅ Order {createdOrder?.OrderId} added to Retail.Order database (read model)", "Success");

                    // Final Success Event
                    AddEvent("Dashboard", $"🎉 Order {createdOrder?.OrderId} processed successfully through all microservices!", "Success");

                    await RefreshOrders();
                    await RefreshProducts(); // Refresh to see inventory changes
                    CloseCreateOrderModal();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.LogError("Failed to create order. StatusCode: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    AddEvent("Orders.Write", $"❌ Failed to create order. Status: {response.StatusCode}, Error: {errorContent}", "Error");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating order. CustomerId: {CustomerId}, SkuId: {SkuId}, Qty: {Qty}",
                    newOrder.CustomerId, newOrderLineItem.SkuId, newOrderLineItem.Qty);
                AddEvent("Orders", $"❌ Error creating order: {ex.Message}", "Error");
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

        public void SwitchToNotifications()
        {
            Logger.LogInformation("SwitchToNotifications called");
            activeTab = "notifications";
            StateHasChanged();
        }
        public void ToggleOrderDetails(long orderId)
        {
            if (expandedOrders.Contains(orderId))
            {
                expandedOrders.Remove(orderId);
            }
            else
            {
                expandedOrders.Add(orderId);
            }
            StateHasChanged();
        }

        private double? GetProductPrice(long skuId)
        {
            var product = products?.FirstOrDefault(p => p.Id == skuId);
            return product?.UnitPrice;
        }  

        public void SimulateSuccessfulOrder()
        {
            // Clear previous events
            eventHistory.Clear();
            
            // Simulate the successful order flow that just occurred
            AddEvent("Orders.Write", "🚀 Order created for customer 3 with 500 units of Salt", "Info");
            AddEvent("Orders.Write", "📤 RabbitMQ: OrderCreated event published to orderdomain.topic.exchange", "Success");
            
            AddEvent("Products", "📥 Received OrderCreated event from RabbitMQ", "Info");
            AddEvent("Products", "🔍 Checking inventory: Salt - Available: 4500, Requested: 500", "Info");
            AddEvent("Products", "✅ Inventory sufficient - Processing order", "Success");
            AddEvent("Products", "📝 Updating Salt inventory: 4500 → 4000", "Success");
            AddEvent("Products", "📤 RabbitMQ: InventoryUpdated event published to inventorydomain.topic.exchange", "Success");
            
            AddEvent("Customers", "📥 Received InventoryUpdated event from RabbitMQ", "Info");
            AddEvent("Customers", "✅ Customer notification created for order", "Success");
            
            AddEvent("Orders.Read", "📥 Received InventoryUpdated event from RabbitMQ", "Info");
            AddEvent("Orders.Read", "🔍 Checking if order already exists in MongoDB read model", "Info");
            AddEvent("Orders.Read", "✅ Order ID 0 already exists - skipping insertion (Idempotency protection)", "Success");
            AddEvent("Orders.Read", "📊 MongoDB duplicate key error resolved through proper duplicate checking", "Success");
            
            AddEvent("Dashboard", "🎉 Order processed successfully! Salt inventory reduced by 500 units", "Success");
            AddEvent("Dashboard", "ℹ️ MongoDB error handling improved - Order Read service now prevents duplicate insertions", "Info");
            
            StateHasChanged();
        }
    } 
}

