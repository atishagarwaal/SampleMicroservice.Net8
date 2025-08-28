# 🏪 Retail Microservices Dashboard

A modern, clean, and functional dashboard for monitoring and managing retail microservices operations. This dashboard showcases the event-driven architecture and demonstrates how different microservices communicate through RabbitMQ.

## ✨ Features

### 🔧 Service Monitoring
- **Real-time Service Status**: Monitor all microservices with visual indicators
- **Health Checks**: Automatic service health monitoring via Swagger endpoints
- **Service Icons**: Intuitive icons for each service type
- **Status Tracking**: Running/Stopped status with color-coded indicators

### 📦 Business Operations
- **Products & Inventory Management**: View products, adjust inventory levels
- **Order Processing**: Create new orders with real-time validation
- **Customer Management**: Access customer information
- **Data Refresh**: Real-time data updates from microservices

### 🔄 Event-Driven Architecture Showcase
- **Message Flow Visualization**: See how services communicate
- **Order Processing Events**: Real-time event timeline showing order flow
- **Service Communication**: Track events across all microservices
- **RabbitMQ Integration**: Demonstrates asynchronous messaging patterns

## 🚀 Getting Started

### Prerequisites
1. **RabbitMQ**: Running locally on port 15672
2. **Microservices**: All services should be running and accessible
3. **Database**: SQL Server with retail databases populated

### Running the Dashboard
1. **Start the UI**:
```bash
cd Retail.UI
dotnet run
```

2. **Navigate to**: `https://localhost:7000`

3. **Access Dashboard**: Click "Dashboard" in the navigation

## 📊 Dashboard Sections

### 1. Service Status
- **BFF Service** (Port 7004): API Gateway and aggregation
- **Customer Service** (Port 7001): Customer management
- **Order Read Service** (Port 7005): Order queries
- **Order Write Service** (Port 7002): Order creation
- **Product Service** (Port 7003): Inventory management

### 2. Business Data Tabs
- **Products & Inventory**: View products, adjust stock levels
- **Orders**: View order history, create new orders
- **Customers**: View customer information

### 3. Message Flow
Visual representation of service communication patterns:
- BFF → Customer Service
- BFF → Order Read Service  
- BFF → Product Service
- Order Write → Product Service

### 4. Order Processing Events
Real-time timeline showing how orders flow through the system:
- Order creation events
- Inventory updates
- Customer notifications
- Service communication events

## 🎯 Demonstrating Event-Driven Architecture

### Creating an Order
1. **Navigate to Orders tab**
2. **Click "Create Order"**
3. **Fill in the form**:
   - Customer ID (must exist in system)
   - Product ID (must exist in system)
   - Quantity (must not exceed inventory)
4. **Click "Create Order"**

### What Happens Behind the Scenes
1. **Order Write Service** receives the order
2. **Product Service** validates inventory and updates stock
3. **Customer Service** updates customer order history
4. **Order Read Service** updates read model
5. **Events are published** to RabbitMQ for other services

### Event Timeline
Watch the real-time event timeline to see:
- ✅ **Success Events**: Green indicators for successful operations
- ❌ **Error Events**: Red indicators for failures
- ℹ️ **Info Events**: Blue indicators for informational messages

## 🔧 Technical Implementation

### Architecture
- **Blazor Server**: Modern web framework for interactive UI
- **HTTP Client**: REST API communication with microservices
- **Event Tracking**: In-memory event history with real-time updates
- **CSS Grid**: Responsive design with modern styling

### Data Flow
```
Dashboard → HTTP Client → Microservice API → Database
                ↓
            Event Tracking → UI Updates
```

### Service Communication
```
Order Write → RabbitMQ → Product Service (Inventory Update)
                ↓
            Customer Service (Order History)
                ↓
            Order Read Service (Read Model)
```

## 🎨 UI Components

### Service Cards
- **Running Status**: Green border and status indicator
- **Stopped Status**: Red border and status indicator
- **Hover Effects**: Smooth animations and visual feedback

### Data Tables
- **Responsive Design**: Works on all screen sizes
- **Hover Effects**: Row highlighting for better UX
- **Action Buttons**: Contextual actions for each row

### Modals
- **Inventory Adjustment**: Update product stock levels
- **Order Creation**: Create new orders with validation
- **Form Validation**: Real-time input validation

## 📱 Responsive Design

The dashboard is fully responsive and works on:
- **Desktop**: Full feature set with side-by-side layouts
- **Tablet**: Optimized layouts for medium screens
- **Mobile**: Stacked layouts for small screens

## 🚨 Troubleshooting

### Common Issues
1. **Services Not Starting**: Check RabbitMQ and database connections
2. **Data Not Loading**: Verify microservice endpoints are accessible
3. **Events Not Showing**: Ensure all services are running

### Debug Information
- Check browser console for JavaScript errors
- Review service logs for API failures
- Verify RabbitMQ queue status

## 🔮 Future Enhancements

- **Real-time WebSocket**: Live updates without refresh
- **Advanced Filtering**: Search and filter capabilities
- **Export Functionality**: Data export to CSV/Excel
- **User Authentication**: Role-based access control
- **Performance Metrics**: Service response time monitoring

## 📚 Learning Resources

- **Microservices Pattern**: Domain-driven design principles
- **Event-Driven Architecture**: Asynchronous communication patterns
- **RabbitMQ**: Message queuing and routing
- **Blazor**: Modern web development with C#

---

The dashboard is now clean, functional, and focused on real business value while showcasing the power of event-driven microservices architecture! 🎉
