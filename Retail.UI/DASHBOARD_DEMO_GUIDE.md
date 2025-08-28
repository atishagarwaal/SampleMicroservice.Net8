# 🎯 **Dashboard Demo Guide - Event-Driven Architecture Showcase**

## 🚀 **Getting Started**

### **1. Start Your Services**
```bash
# Start in this order:
1. MessagingInfrastructure (creates RabbitMQ topology)
2. Retail.Customers (Port 7001)
3. Retail.Products (Port 7003) 
4. Retail.Orders.Write (Port 7002)
5. Retail.Orders.Read (Port 7005)
6. Retail.BFF (Port 7004)
7. Retail.UI (Port 7000) - This dashboard
```

### **2. Access the Dashboard**
- Navigate to: `https://localhost:7000`
- Click **"Dashboard"** in the navigation menu

---

## 📊 **Dashboard Overview**

### **Service Status Section**
- **Real-time monitoring** of all microservices
- **Color-coded status**: Green (Running), Red (Stopped)
- **Service icons** for easy identification
- **Automatic health checks** via Swagger endpoints

### **Business Data Tabs**
- **📦 Products & Inventory**: View products, adjust stock levels
- **📋 Orders**: View order history, create new orders
- **👥 Customers**: View customer information

### **Event Flow Visualization**
- **Message Flow**: How services communicate
- **Order Processing Events**: Real-time event timeline

---

## 🎯 **Demo Scenario: Creating an Order**

### **Step 1: View Current Data**
1. **Go to Products tab** - See available inventory
2. **Go to Customers tab** - Note customer IDs
3. **Go to Orders tab** - See current order history

### **Step 2: Create a New Order**
1. **Click "Create Order"** button in Orders tab
2. **Select Customer** from dropdown (e.g., Customer ID 1)
3. **Select Product** from dropdown (e.g., Product ID 1)
4. **Enter Quantity** (ensure it doesn't exceed inventory)
5. **Review Order Summary** - shows total cost and stock validation
6. **Click "Create Order"**

### **Step 3: Watch the Event Flow**
The dashboard will show **real-time events** as the order processes:

```
✅ Dashboard: Opening order creation modal
✅ Orders: Creating order for customer 1 with X units of Product Name
✅ Orders.Write: Order created successfully with ID X
✅ Products: Inventory updated for Product Name (reduced by X)
✅ Customers: Customer 1 order history updated  
✅ Orders.Read: Order X added to read model
```

---

## 🔄 **Event-Driven Architecture in Action**

### **What Happens Behind the Scenes**

1. **Order Write Service** receives the order
2. **Product Service** validates inventory and updates stock
3. **Customer Service** updates customer order history
4. **Order Read Service** updates read model
5. **Events are published** to RabbitMQ for other services

### **Service Communication Flow**
```
BFF Service → Customer Service (for customer data)
BFF Service → Order Read Service (for order data)  
BFF Service → Product Service (for product data)
Order Write Service → Product Service (for inventory updates)
Product Service → Customer Service & Order Read Service (via RabbitMQ events)
```

---

## 📦 **Inventory Management Demo**

### **Adjust Product Inventory**
1. **Go to Products tab**
2. **Click "Adjust" button** on any product row
3. **Enter new inventory value**
4. **Click "Update Inventory"**
5. **Watch events** show inventory update

### **Inventory Status Indicators**
- **🟢 High**: Green (Inventory > 10)
- **🟡 Medium**: Yellow (Inventory 1-10)  
- **🔴 Low**: Red (Inventory = 0)

---

## 🎨 **UI Features**

### **Interactive Controls**
- **Tab Navigation**: Switch between Products, Orders, Customers
- **Refresh Buttons**: Update data from microservices
- **Action Buttons**: Adjust inventory, create orders
- **Modal Dialogs**: User-friendly forms

### **Real-Time Updates**
- **Event Timeline**: Live order processing events
- **Service Status**: Automatic health monitoring
- **Data Refresh**: Real-time business data updates

### **Responsive Design**
- **Desktop**: Full feature set with side-by-side layouts
- **Tablet**: Optimized layouts for medium screens
- **Mobile**: Stacked layouts for small screens

---

## 🚨 **Troubleshooting**

### **Common Issues**

1. **Services Not Starting**
   - Check RabbitMQ is running on port 15672
   - Verify database connections
   - Check service ports in launchSettings.json

2. **Data Not Loading**
   - Ensure microservices are accessible
   - Check API endpoints in browser
   - Verify service URLs in code-behind

3. **Buttons Not Working**
   - Check browser console for JavaScript errors
   - Ensure all services are running
   - Verify HTTP client configuration

### **Debug Information**
- **Browser Console**: JavaScript and network errors
- **Service Logs**: API failures and exceptions
- **Event Timeline**: Real-time operation tracking
- **Service Status**: Health check results

---

## 🎯 **Demo Script for Presentations**

### **Opening (2 minutes)**
"Welcome to our Retail Microservices Dashboard. This demonstrates a modern, event-driven architecture where services communicate asynchronously through RabbitMQ."

### **Service Overview (1 minute)**
"Here we can see all our microservices running - BFF, Customers, Products, Orders Write, and Orders Read. Each has a specific domain responsibility."

### **Business Operations (2 minutes)**
"Let me show you how we manage inventory and create orders. Watch how the system validates stock levels and processes orders in real-time."

### **Event Flow Demo (3 minutes)**
"Now let's create an order and watch the event-driven architecture in action. Notice how events flow through the system and update multiple services."

### **Architecture Benefits (1 minute)**
"This architecture provides scalability, fault tolerance, and real-time processing. Services can be deployed independently and communicate reliably through message queues."

---

## 🔮 **Advanced Features**

### **Event History**
- **Last 50 events** stored in memory
- **Real-time updates** as operations occur
- **Status indicators** for success/error/info
- **Timestamp tracking** for debugging

### **Data Validation**
- **Input validation** on all forms
- **Business rule enforcement** (e.g., inventory checks)
- **Error handling** with user-friendly messages
- **Success feedback** for completed operations

### **Performance Monitoring**
- **Service response times** via health checks
- **Data loading indicators** for user feedback
- **Error tracking** for operational insights
- **Event correlation** for debugging

---

## 📚 **Learning Outcomes**

### **For Developers**
- **Microservices patterns** and best practices
- **Event-driven architecture** implementation
- **RabbitMQ integration** and configuration
- **Blazor development** with modern UI

### **For Business Users**
- **Real-time business operations** monitoring
- **Inventory management** workflows
- **Order processing** end-to-end
- **Service health** and reliability

### **For Architects**
- **System design** principles
- **Service communication** patterns
- **Scalability** considerations
- **Fault tolerance** strategies

---

## 🎉 **Success Metrics**

### **Dashboard Functionality**
- ✅ All buttons and controls working
- ✅ Real-time data loading from microservices
- ✅ Interactive inventory management
- ✅ Order creation with validation
- ✅ Event timeline showing system flow

### **Event-Driven Architecture**
- ✅ Services communicating via RabbitMQ
- ✅ Real-time event tracking
- ✅ Order processing workflow
- ✅ Inventory updates across services
- ✅ Customer history maintenance

---

**The dashboard is now a powerful demonstration tool that showcases the complete event-driven microservices architecture!** 🚀

**Next step**: Start your services and run through this demo to see the magic happen! ✨
