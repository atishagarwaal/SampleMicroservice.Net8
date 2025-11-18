using System;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InventoryUpdatedEventNameSpace;
using Retail.Orders.Read.src.CleanArchitecture.Application.EventHandlers;
using Retail.Orders.Read.src.CleanArchitecture.Domain.Entities;
using Retail.Orders.Read.src.CleanArchitecture.Infrastructure.Interfaces;
using LineItemEntity = Retail.Orders.Read.src.CleanArchitecture.Domain.Entities.LineItem;
using LineItemEvent = InventoryUpdatedEventNameSpace.LineItem;

namespace Retail.Orders.Read.ComponentTests
{
    /// <summary>
    /// Unit tests for InventoryUpdatedEventHandler class.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    [TestCategory("InventoryUpdatedEventHandler")]
    public sealed class InventoryUpdatedEventHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
        private Mock<IServiceScope> _mockServiceScope = null!;
        private Mock<IServiceProvider> _mockServiceProvider = null!;
        private Mock<IOrderRepository> _mockOrderRepository = null!;
        private Mock<ILogger<InventoryUpdatedEventHandler>> _mockLogger = null!;
        private Mock<IMetricsService> _mockMetrics = null!;
        private InventoryUpdatedEventHandler _handler = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockLogger = new Mock<ILogger<InventoryUpdatedEventHandler>>();
            _mockMetrics = new Mock<IMetricsService>();

            _mockServiceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceScope
                .Setup(x => x.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IUnitOfWork)))
                .Returns(_mockUnitOfWork.Object);

            _mockUnitOfWork
                .Setup(x => x.Orders)
                .Returns(_mockOrderRepository.Object);

            // Setup metrics mock
            _mockMetrics.Setup(x => x.TrackDuration(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(Mock.Of<IDisposable>());

            _handler = new InventoryUpdatedEventHandler(
                _mockUnitOfWork.Object,
                _mockServiceScopeFactory.Object,
                _mockLogger.Object,
                _mockMetrics.Object);
        }

        [TestMethod]
        public async Task HandleAsync_WithNullEvent_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () => await _handler.HandleAsync(null!));
        }

        [TestMethod]
        public async Task HandleAsync_WhenOrderAlreadyExists_SkipsInsertion()
        {
            // Arrange
            var existingOrder = new Order
            {
                Id = 1,
                CustomerId = 100
            };

            var inventoryUpdatedEvent = new InventoryUpdatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = (double)100.50m,
                LineItems = Array.Empty<LineItemEvent>()
            };

            _mockOrderRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(existingOrder);

            // Act
            Func<Task> act = async () => await _handler.HandleAsync(inventoryUpdatedEvent);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task HandleAsync_WithValidEvent_AddsOrderSuccessfully()
        {
            // Arrange
            var inventoryUpdatedEvent = new InventoryUpdatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = (double)100.50m,
                LineItems = Array.Empty<LineItemEvent>()
            };

            _mockOrderRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync((Order?)null);

            _mockOrderRepository
                .Setup(x => x.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order order) => order);

            // Act
            Func<Task> act = async () => await _handler.HandleAsync(inventoryUpdatedEvent);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task HandleAsync_WithEventContainingLineItems_AddsOrderWithLineItems()
        {
            // Arrange
            var lineItems = new[]
            {
                new LineItemEvent { SkuId = 1, Qty = 2 },
                new LineItemEvent { SkuId = 2, Qty = 3 }
            };

            var inventoryUpdatedEvent = new InventoryUpdatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = (double)100.50m,
                LineItems = lineItems
            };

            _mockOrderRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync((Order?)null);

            _mockOrderRepository
                .Setup(x => x.AddAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order order) => order);

            // Act
            Func<Task> act = async () => await _handler.HandleAsync(inventoryUpdatedEvent);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task HandleAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inventoryUpdatedEvent = new InventoryUpdatedEvent
            {
                OrderId = 1,
                CustomerId = 100,
                OrderDate = DateTimeOffset.UtcNow,
                TotalAmount = (double)100.50m,
                LineItems = Array.Empty<LineItemEvent>()
            };

            var expectedException = new InvalidOperationException("Repository error");
            _mockOrderRepository
                .Setup(x => x.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync((Order?)null);

            _mockOrderRepository
                .Setup(x => x.AddAsync(It.IsAny<Order>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _handler.HandleAsync(inventoryUpdatedEvent));

            exception.Should().Be(expectedException);
            exception.Message.Should().Be("Repository error");
        }
    }
}

